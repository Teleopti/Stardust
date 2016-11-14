﻿using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.FeatureFlags;
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
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Global
{
	[IoCTest]
	[TestFixture]
	public class AreaWithPermissionPathProviderTest : ISetup
	{
		public IAreaWithPermissionPathProvider Target;
		public FakePermissionProvider PermissionProvider;
		public FakeToggleManager ToggleManager;
		public FakeApplicationFunctionsToggleFilter ApplicationFunctionsToggleFilter;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.AddModule(new WebAppModule(configuration));
			system.UseTestDouble<FakePermissionProvider>().For<IPermissionProvider>();
			system.UseTestDouble<FakeToggleManager>().For<IToggleManager>();
			system.UseTestDouble<FakeApplicationFunctionsToggleFilter>().For<IApplicationFunctionsToggleFilter>();

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
			ToggleManager.Enable(Toggles.WfmTeamSchedule_AbsenceReporting_35995);
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
			ToggleManager.Enable(Toggles.WfmTeamSchedule_AbsenceReporting_35995);

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
		public void ShouldHaveIntradayAreaWhenFeatureEnabledAndPermitted()
		{
			ApplicationFunctionsToggleFilter
				.AddFakeFunction(new ApplicationFunction { FunctionCode = DefinedRaptorApplicationFunctionPaths.WebIntraday }
			, o => true);

			PermissionProvider.Enable();
			ToggleManager.Enable(Toggles.Wfm_Intraday_38074);
			PermissionProvider.Permit(DefinedRaptorApplicationFunctionPaths.WebIntraday);

			var areas = Target.GetWfmAreasWithPermissions();

			areas.Single().Path.Should().Be(DefinedRaptorApplicationFunctionPaths.WebIntraday);
			areas.Single().Name.Invoke().Should().Be(Resources.Intraday);
			areas.Single().InternalName.Should().Be("intraday");
		}

		[Test]
		public void ShouldNotHaveIntradayAreaWhenFeatureIsDisabled()
		{
			PermissionProvider.Enable();
			ToggleManager.Disable(Toggles.Wfm_Intraday_38074);
			PermissionProvider.Permit(DefinedRaptorApplicationFunctionPaths.WebIntraday);

			var areas = Target.GetWfmAreasWithPermissions();

			areas.Count().Should().Be(0);
		}

		[Test]
		public void ShouldNotHaveIntradayAreaWhenItIsNotPermitted()
		{
			PermissionProvider.Enable();
			ToggleManager.Enable(Toggles.Wfm_Intraday_38074);

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

			ToggleManager.Enable(Toggles.Wfm_Requests_Basic_35986);

			var areas = Target.GetWfmAreasWithPermissions();

			areas.Count().Should().Be(1);
			areas.Single().Path.Should().Be(DefinedRaptorApplicationFunctionPaths.WebRequests);
		}
	}
}
