using Autofac;
using Teleopti.Analytics.Etl.Common.Transformer;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Analytics.Etl.IntegrationTest
{
	public class FakeContainerHolder : IContainerHolder
	{
		public FakeContainerHolder()
		{
			ToggleManager = new FakeToggleManager();
			TenantLogonInfoLoader = new FakeTenantLogonInfoLoader();
		}
		public IToggleManager ToggleManager { get; private set; }
		public ITenantLogonInfoLoader TenantLogonInfoLoader { get; private set; }
		public IComponentContext IocContainer { get; set; }


		public void EnableToggle(Toggles toggle)
		{
			((FakeToggleManager)ToggleManager).Enable(toggle);
		}

		public void DisableToggle(Toggles toggle)
		{
			((FakeToggleManager)ToggleManager).Enable(toggle);
		}
	}
}
