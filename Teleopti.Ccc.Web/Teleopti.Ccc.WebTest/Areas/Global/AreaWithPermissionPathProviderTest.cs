using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Ccc.Web.Core.IoC;
using Teleopti.Ccc.WebTest.Areas.Permissions;

namespace Teleopti.Ccc.WebTest.Areas.Global
{
	[IoCTest]
	[TestFixture]
	public class AreaWithPermissionPathProviderTest : IIsolateSystem, IExtendSystem
	{
		public IAreaWithPermissionPathProvider Target;
		public FakePermissionProvider PermissionProvider;
		public FakeToggleManager ToggleManager;
		public FakeApplicationFunctionsToggleFilter ApplicationFunctionsToggleFilter;
		
		public void Extend(IExtend extend, IIocConfiguration configuration)
		{
			extend.AddModule(new WebAppModule(configuration));
		}

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<FakePermissionProvider>().For<IPermissionProvider>();
			isolate.UseTestDouble<FakeToggleManager>().For<IToggleManager>();
			isolate.UseTestDouble<FakeApplicationFunctionsToggleFilter>().For<IApplicationFunctionsToggleFilter>();
		}

		[Test]
		public void ShouldHaveRtaAreaWhenFeatureEnabledAndPermitted()
		{
			ApplicationFunctionsToggleFilter
				.AddFakeFunction(new ApplicationFunction { FunctionCode = DefinedRaptorApplicationFunctionPaths.RealTimeAdherenceOverview }
			, o => true);

			PermissionProvider.Enable();
			PermissionProvider.Permit(DefinedRaptorApplicationFunctionPaths.RealTimeAdherenceOverview);

			var areas = Target.GetWfmAreasWithPermissions();

			areas.Single().Path.Should().Be(DefinedRaptorApplicationFunctionPaths.RealTimeAdherenceOverview);
			areas.Single().Name.Invoke().Should().Be(Resources.RealTimeAdherence);
			areas.Single().InternalName.Should().Be("rta");
		}

		[Test]
		public void ShouldNotHaveRtaAreaWhenItIsNotPermitted()
		{
			PermissionProvider.Enable();

			var areas = Target.GetWfmAreasWithPermissions();

			areas.Count().Should().Be(0);
		}

		[Test]
		public void ShouldHaveTeamsWhenPermittedAndFeatureEnabled()
		{
			ApplicationFunctionsToggleFilter
				.AddFakeFunction(new ApplicationFunction { FunctionCode = DefinedRaptorApplicationFunctionPaths.MyTeamSchedules }
			, o => true);

			PermissionProvider.Enable();
			PermissionProvider.Permit(DefinedRaptorApplicationFunctionPaths.MyTeamSchedules);

			var areas = Target.GetWfmAreasWithPermissions();

			areas.Single().Path.Should().Be(DefinedRaptorApplicationFunctionPaths.MyTeamSchedules);
			areas.Single().Name.Invoke().Should().Be(Resources.Teams);
			areas.Single().InternalName.Should().Be("teams");
		}

		[Test]
		public void ShouldNotHaveTeamsWhenItNotPermitted()
		{
			PermissionProvider.Enable();

			var areas = Target.GetWfmAreasWithPermissions();

			areas.Count().Should().Be(0);
		}

		[Test]
		public void ShouldReturnMyTeamScheduleWhenWfmTeamsIsReleased()
		{
			ApplicationFunctionsToggleFilter
				.AddFakeFunction(new ApplicationFunction { FunctionCode = DefinedRaptorApplicationFunctionPaths.MyTeamSchedules }
			, o => true);

			PermissionProvider.Enable();
			PermissionProvider.Permit(DefinedRaptorApplicationFunctionPaths.MyTeamSchedules);
			var areas = Target.GetWfmAreasWithPermissions();

			areas.Count().Should().Be(1);
			areas.First().Path.Should().Be.EqualTo(DefinedRaptorApplicationFunctionPaths.MyTeamSchedules);
		}

		[Test]
		public void ShouldHaveIntradayAreaWhenPermitted()
		{
			ApplicationFunctionsToggleFilter
				.AddFakeFunction(new ApplicationFunction { FunctionCode = DefinedRaptorApplicationFunctionPaths.WebIntraday }
			, o => true);

			PermissionProvider.Enable();
			PermissionProvider.Permit(DefinedRaptorApplicationFunctionPaths.WebIntraday);

			var areas = Target.GetWfmAreasWithPermissions();

			areas.Single().Path.Should().Be(DefinedRaptorApplicationFunctionPaths.WebIntraday);
			areas.Single().Name.Invoke().Should().Be(Resources.Intraday);
			areas.Single().InternalName.Should().Be("intraday");
		}

		[Test]
		public void ShouldNotHaveIntradayAreaWhenItIsNotPermitted()
		{
			PermissionProvider.Enable();

			var areas = Target.GetWfmAreasWithPermissions();

			areas.Count().Should().Be(0);
		}

		[Test]
		public void ShouldHaveStaffingAreaWhenFeaturePermitted()
		{
			ApplicationFunctionsToggleFilter
				.AddFakeFunction(new ApplicationFunction { FunctionCode = DefinedRaptorApplicationFunctionPaths.WebStaffing }
			, o => true);

			PermissionProvider.Enable();
			PermissionProvider.Permit(DefinedRaptorApplicationFunctionPaths.WebStaffing);

			var areas = Target.GetWfmAreasWithPermissions();

			areas.Single().Path.Should().Be(DefinedRaptorApplicationFunctionPaths.WebStaffing);
			areas.Single().Name.Invoke().Should().Be(Resources.WebStaffing);
			areas.Single().InternalName.Should().Be("staffingModule");
		}
		
		[Test]
		public void ShouldNotHaveStaffingAreaWhenItIsNotPermitted()
		{
			PermissionProvider.Enable();
			
			var areas = Target.GetWfmAreasWithPermissions();

			areas.Count().Should().Be(0);
		}

		[Test]
		public void ShouldNotHaveAreasFromNotLicensedModules()
		{
			ApplicationFunctionsToggleFilter
				.AddFakeFunction(new ApplicationFunction { FunctionCode = DefinedRaptorApplicationFunctionPaths.WebRequests }
			, o => true);
			ApplicationFunctionsToggleFilter
				.AddFakeFunction(new ApplicationFunction { FunctionCode = DefinedRaptorApplicationFunctionPaths.SeatPlanner }
			, o => false);

			PermissionProvider.Enable();
			PermissionProvider.Permit(DefinedRaptorApplicationFunctionPaths.WebRequests);
			PermissionProvider.Permit(DefinedRaptorApplicationFunctionPaths.SeatPlanner);

			var areas = Target.GetWfmAreasWithPermissions();

			areas.Count().Should().Be(1);
			areas.Single().Path.Should().Be(DefinedRaptorApplicationFunctionPaths.WebRequests);
		}

		[Test]
		public void ShouldHaveMyTimeAreaWhenPermittedAndFeatureIsEnabled()
		{
			ApplicationFunctionsToggleFilter
				.AddFakeFunction(new ApplicationFunction { FunctionCode = DefinedRaptorApplicationFunctionPaths.MyTimeWeb }
					, o => true);
			PermissionProvider.Enable();
			ToggleManager.Enable(Toggles.Wfm_AddMyTimeLink_45088);
			PermissionProvider.Permit(DefinedRaptorApplicationFunctionPaths.MyTimeWeb);

			var areas = Target.GetWfmAreasWithPermissions();

			areas.Count().Should().Be(1);
		}

		[Test]
		public void ShouldHaveGamificationAreaWhenPermittedAndFeatureIsEnabled()
		{
			ApplicationFunctionsToggleFilter
				.AddFakeFunction(new ApplicationFunction { FunctionCode = DefinedRaptorApplicationFunctionPaths.OpenOptionsPage }
					, o => true);
			PermissionProvider.Enable();
			ToggleManager.Enable(Toggles.WFM_Gamification_Setting_With_External_Quality_Values_45003);
			PermissionProvider.Permit(DefinedRaptorApplicationFunctionPaths.OpenOptionsPage);

			var areas = Target.GetWfmAreasWithPermissions();

			areas.Count().Should().Be(1);
		}

		[Test]
		public void ShouldNotHaveMyTimeAreaWhenFeatureIsEnabledButWithoutPermission()
		{
			ApplicationFunctionsToggleFilter
				.AddFakeFunction(new ApplicationFunction { FunctionCode = DefinedRaptorApplicationFunctionPaths.MyTimeWeb }
					, o => true);
			PermissionProvider.Enable();
			ToggleManager.Enable(Toggles.Wfm_AddMyTimeLink_45088);

			var areas = Target.GetWfmAreasWithPermissions();

			areas.Count().Should().Be(0);
		}

		[Test]
		public void ShouldNotHaveMyTimeAreaWhenFeatureIsDisabled()
		{
			PermissionProvider.Enable();
			ToggleManager.Disable(Toggles.Wfm_AddMyTimeLink_45088);
			PermissionProvider.Permit(DefinedRaptorApplicationFunctionPaths.MyTimeWeb);

			var areas = Target.GetWfmAreasWithPermissions();

			areas.Count().Should().Be(0);
		}

		[Test]
		public void ShouldNotHavePeopleAreaWhenFeatureDisabled()
		{
			ApplicationFunctionsToggleFilter
				.AddFakeFunction(new ApplicationFunction { FunctionCode = DefinedRaptorApplicationFunctionPaths.PeopleAccess }
					, o => true);

			PermissionProvider.Enable();
			PermissionProvider.Permit(DefinedRaptorApplicationFunctionPaths.PeopleAccess);
			ToggleManager.Disable(Toggles.Wfm_PeopleWeb_PrepareForRelease_47766);

			var areas = Target.GetWfmAreasWithPermissions();

			areas.Count().Should().Be(0);
		}

		[Test]
		public void ShouldHavePeopleAreaWhenFeatureEnabledAndPermitted()
		{
			ApplicationFunctionsToggleFilter
				.AddFakeFunction(new ApplicationFunction { FunctionCode = DefinedRaptorApplicationFunctionPaths.PeopleAccess }
					, o => true);

			PermissionProvider.Enable();
			PermissionProvider.Permit(DefinedRaptorApplicationFunctionPaths.PeopleAccess);
			ToggleManager.Enable(Toggles.Wfm_PeopleWeb_PrepareForRelease_47766);

			var areas = Target.GetWfmAreasWithPermissions();

			areas.Single().Path.Should().Be(DefinedRaptorApplicationFunctionPaths.PeopleAccess);
			areas.Single().Name.Invoke().Should().Be(Resources.People);
			areas.Single().InternalName.Should().Be("people");
		}

		[Test]
		public void ShouldNotHaveWebForecastWhenFeatureDisabled()
		{
			ApplicationFunctionsToggleFilter
				.AddFakeFunction(new ApplicationFunction { FunctionCode = DefinedRaptorApplicationFunctionPaths.WebForecasts }
					, o => true);

			PermissionProvider.Enable();
			PermissionProvider.Permit(DefinedRaptorApplicationFunctionPaths.WebForecasts);
			ToggleManager.Disable(Toggles.WFM_Forecaster_Preview_74801);

			var areas = Target.GetWfmAreasWithPermissions();

			areas.Count().Should().Be(0);
		}

		[Test]
		public void ShouldHaveWebForecastWhenFeatureEnabledAndPermitted()
		{
			ApplicationFunctionsToggleFilter
				.AddFakeFunction(new ApplicationFunction { FunctionCode = DefinedRaptorApplicationFunctionPaths.WebForecasts }
					, o => true);

			PermissionProvider.Enable();
			PermissionProvider.Permit(DefinedRaptorApplicationFunctionPaths.WebForecasts);
			ToggleManager.Enable(Toggles.WFM_Forecaster_Preview_74801);

			var areas = Target.GetWfmAreasWithPermissions();

			areas.Single().Path.Should().Be(DefinedRaptorApplicationFunctionPaths.WebForecasts);
			areas.Single().Name.Invoke().Should().Be(Resources.Forecasts);
			areas.Single().InternalName.Should().Be("forecast");
		}

		[Test]
		public void ShouldHaveGamificationWhenFeatureEnabledAndPermitted()
		{
			ApplicationFunctionsToggleFilter
				.AddFakeFunction(new ApplicationFunction { FunctionCode = DefinedRaptorApplicationFunctionPaths.Gamification }
					, o => true);
			ToggleManager.Enable(Toggles.WFM_Gamification_Permission_76546);
			PermissionProvider.Enable();
			PermissionProvider.Permit(DefinedRaptorApplicationFunctionPaths.Gamification);

			var areas = Target.GetWfmAreasWithPermissions();

			areas.Single().Path.Should().Be(DefinedRaptorApplicationFunctionPaths.Gamification);
			areas.Single().Name.Invoke().Should().Be(Resources.Gamification);
			areas.Single().InternalName.Should().Be("gamification");
		}

		[Test]
		public void ShouldNotHaveGamificationWhenItIsNotPermitted()
		{
			PermissionProvider.Enable();

			var areas = Target.GetWfmAreasWithPermissions();

			areas.Count().Should().Be(0);
		}
	}
}
