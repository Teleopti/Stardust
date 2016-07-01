using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Ccc.Web.Core.IoC;

namespace Teleopti.Ccc.WebTest.Areas.Global
{
	[IoCTest]
	[TestFixture]
	public class AreaWithPermissionPathProviderTest : ISetup
	{
		public IAreaWithPermissionPathProvider Target;
		public FakePermissionProvider PermissionProvider;
		public FakeToggleManager ToggleManager;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.AddModule(new WebAppModule(configuration));
			system.UseTestDouble<FakePermissionProvider>().For<IPermissionProvider>();
			system.UseTestDouble<FakeToggleManager>().For<IToggleManager>();
			
		}

		[Test]
		public void ShouldHaveRtaAreaWhenFeatureEnabledAndPermitted()
		{
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
		public void ShouldHaveMyTeamWhenPermittedAndFeatureEnabled()
		{
			PermissionProvider.Enable();
			ToggleManager.Enable(Toggles.WfmTeamSchedule_AbsenceReporting_35995);
			PermissionProvider.Permit(DefinedRaptorApplicationFunctionPaths.MyTeamSchedules);

			var areas = Target.GetWfmAreasWithPermissions();

			areas.Single().Path.Should().Be(DefinedRaptorApplicationFunctionPaths.MyTeamSchedules);
			areas.Single().Name.Invoke().Should().Be(Resources.MyTeam);
			areas.Single().InternalName.Should().Be("myTeamSchedule");
		}

		[Test]
		public void ShouldNotHaveMyTeamWhenItNotPermitted()
		{
			PermissionProvider.Enable();
			ToggleManager.Enable(Toggles.WfmTeamSchedule_AbsenceReporting_35995);

			var areas = Target.GetWfmAreasWithPermissions();

			areas.Count().Should().Be(0);
		}

		[Test]
		public void ShouldNotHaveMyTeamWhenFeatureIsDisabled()
		{
			PermissionProvider.Enable();
			ToggleManager.Disable(Toggles.WfmTeamSchedule_AbsenceReporting_35995);
			PermissionProvider.Permit(DefinedRaptorApplicationFunctionPaths.MyTeamSchedules);

			var areas = Target.GetWfmAreasWithPermissions();

			areas.Count().Should().Be(0);
		}

		[Test]
		public void ShouldReturnMyTeamScheduleWhenWfmTeamScheduleIsReleased()
		{
			PermissionProvider.Enable();
			PermissionProvider.Permit(DefinedRaptorApplicationFunctionPaths.MyTeamSchedules);
			var areas = Target.GetWfmAreasWithPermissions();

			areas.Count().Should().Be(1);
			areas.First().Path.Should().Be.EqualTo(DefinedRaptorApplicationFunctionPaths.MyTeamSchedules);
		}
		
		[Test]
		public void ShouldHaveIntradayAreaWhenFeatureEnabledAndPermitted()
		{
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

	}
}
