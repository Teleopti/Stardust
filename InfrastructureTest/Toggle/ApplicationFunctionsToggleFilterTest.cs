using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.InfrastructureTest.Toggle
{
	public class ApplicationFunctionsToggleFilterTest
	{
		[Test]
		[TestCase(Toggles.Report_Remove_Realtime_AuditTrail_44006, DefinedRaptorApplicationFunctionPaths.ScheduleAuditTrailReport)]
		[TestCase(Toggles.Report_Remove_Realtime_Scheduled_Time_Per_Activity_45560, DefinedRaptorApplicationFunctionPaths.ScheduledTimePerActivityReport)]
		[TestCase(Toggles.Report_Remove_Realtime_Scheduled_Time_vs_Target_45559, DefinedRaptorApplicationFunctionPaths.ScheduleTimeVersusTargetTimeReport)]
		public void ShouldHideApplicationFunctionWithToggleOn(Toggles toggle, string applicationFunctionPath)
		{
			var toggleManager = new FakeToggleManager(toggle);

			var factory = new DefinedRaptorApplicationFunctionFactory();
			var dataSource = new FakeCurrentDatasource("TestDataSource");
			var functionsProvider = new FakeLicensedFunctionProvider();
			functionsProvider.ClearLicensedFunctions(dataSource.CurrentName());
			functionsProvider.SetLicensedFunctions(factory.ApplicationFunctions);
			var functionRepository = new FakeApplicationFunctionRepository();
			factory.ApplicationFunctions.ForEach(functionRepository.Add);

			var target = new ApplicationFunctionsToggleFilter(
				new ApplicationFunctionsProvider(functionRepository,
					functionsProvider, 
					dataSource), toggleManager);

			var functions = target.FilteredFunctions();

			functions.FindByFunctionPath(applicationFunctionPath).Hidden.Should().Be.True();
		}

		[Test]
		public void ShouldHideAllOnlineReports()
		{
			var toggleManager = new FakeToggleManager();
			toggleManager.Enable(Toggles.Report_Remove_Realtime_Scheduled_Time_vs_Target_45559);
			toggleManager.Enable(Toggles.Report_Remove_Realtime_Scheduled_Time_Per_Activity_45560);
			toggleManager.Enable(Toggles.Report_Remove_Realtime_AuditTrail_44006);

			var factory = new DefinedRaptorApplicationFunctionFactory();
			var dataSource = new FakeCurrentDatasource("TestDataSource");
			var functionsProvider = new FakeLicensedFunctionProvider();
			functionsProvider.ClearLicensedFunctions(dataSource.CurrentName());
			functionsProvider.SetLicensedFunctions(factory.ApplicationFunctions);
			var functionRepository = new FakeApplicationFunctionRepository();
			factory.ApplicationFunctions.ForEach(functionRepository.Add);

			var target = new ApplicationFunctionsToggleFilter(
				new ApplicationFunctionsProvider(functionRepository,
					functionsProvider,
					dataSource), toggleManager);

			var functions = target.FilteredFunctions();

			var onlineReportFunctions = functions.Functions.SelectMany(f =>
				f.ChildFunctions.Where(x =>
					x.Function.LocalizedFunctionDescription == Resources.OnlineReports));
			onlineReportFunctions.Should().Not.Be.Empty();
			onlineReportFunctions.All(f => f.Hidden).Should().Be.True();
		}

		[Test]
		[TestCase(DefinedRaptorApplicationFunctionPaths.ScheduleAuditTrailWebReport)]
		[TestCase(DefinedRaptorApplicationFunctionPaths.Gamification)]
		[TestCase(DefinedRaptorApplicationFunctionPaths.ChatBot)]
		public void ShouldHideApplicationFunctionWithToggleOff(string applicationFunctionPath)
		{
			var toggleManager = new FakeToggleManager();

			var factory = new DefinedRaptorApplicationFunctionFactory();
			var dataSource = new FakeCurrentDatasource("TestDataSource");
			var functionsProvider = new FakeLicensedFunctionProvider();
			functionsProvider.ClearLicensedFunctions(dataSource.CurrentName());
			functionsProvider.SetLicensedFunctions(factory.ApplicationFunctions);
			var functionRepository = new FakeApplicationFunctionRepository();
			factory.ApplicationFunctions.ForEach(functionRepository.Add);

			var target = new ApplicationFunctionsToggleFilter(
				new ApplicationFunctionsProvider(functionRepository,
					functionsProvider,
					dataSource), toggleManager);

			var functions = target.FilteredFunctions();

			functions.FindByFunctionPath(applicationFunctionPath).Hidden.Should()
				.Be.True();
		}

		[Test]
		[TestCase(DefinedRaptorApplicationFunctionPaths.BpoExchange)]
		[TestCase(DefinedRaptorApplicationFunctionPaths.ChatBot)]
		[TestCase(DefinedRaptorApplicationFunctionPaths.PmNextGen)]
		[TestCase(DefinedRaptorApplicationFunctionPaths.PmNextGenViewReport)]
		[TestCase(DefinedRaptorApplicationFunctionPaths.PmNextGenEditReport)]
		public void ShouldHideApplicationFunctionWithoutLicenseOption(string applicationFunctionPath)
		{
			var toggleManager = new TrueToggleManager();
			var factory = new DefinedRaptorApplicationFunctionFactory();
			var dataSource = new FakeCurrentDatasource("TestDataSource");
			var functionsProvider = new FakeLicensedFunctionProvider();
			functionsProvider.ClearLicensedFunctions(dataSource.CurrentName());

			var functionRepository = new FakeApplicationFunctionRepository();
			factory.ApplicationFunctions.ForEach(functionRepository.Add);

			var target = new ApplicationFunctionsToggleFilter(
				new ApplicationFunctionsProvider(functionRepository,
					functionsProvider,
					dataSource), toggleManager);

			var functions = target.FilteredFunctions();

			functions.FindByFunctionPath(applicationFunctionPath).Hidden.Should()
				.Be.True();
		}
	}
}