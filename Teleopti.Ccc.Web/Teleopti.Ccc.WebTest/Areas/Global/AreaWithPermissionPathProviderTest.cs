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
			ToggleManager.Enable(Toggles.Wfm_RTA_34621);
			PermissionProvider.Permit(DefinedRaptorApplicationFunctionPaths.RealTimeAdherenceOverview);

			var areas = Target.GetWfmAreasWithPermissions();

			areas.Single().Path.Should().Be(DefinedRaptorApplicationFunctionPaths.RealTimeAdherenceOverview);
			areas.Single().Name.Invoke().Should().Be(Resources.RealTimeAdherence);
			areas.Single().InternalName.Should().Be("rta");
		}

		[Test]
		public void ShouldNotHaveRtaAreaWhenFeatureIsDisabled()
		{
			PermissionProvider.Enable();
			ToggleManager.Disable(Toggles.Wfm_RTA_34621);
			PermissionProvider.Permit(DefinedRaptorApplicationFunctionPaths.RealTimeAdherenceOverview);

			var areas = Target.GetWfmAreasWithPermissions();

			areas.Count().Should().Be(0);
		}

		[Test]
		public void ShouldNotHaveRtaAreaWhenItIsNotPermitted()
		{
			PermissionProvider.Enable();
			ToggleManager.Enable(Toggles.Wfm_RTA_34621);
			
			var areas = Target.GetWfmAreasWithPermissions();

			areas.Count().Should().Be(0);
		}
	}
}
