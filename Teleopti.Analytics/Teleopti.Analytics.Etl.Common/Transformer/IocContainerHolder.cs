using Autofac;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.Toggle;

namespace Teleopti.Analytics.Etl.Common.Transformer
{
	public interface IContainerHolder
	{
		IToggleManager ToggleManager { get; }
		ITenantLogonInfoLoader TenantLogonInfoLoader { get; }
		IComponentContext IocContainer { get; set; }
	}

	public class IocContainerHolder : IContainerHolder
	{
		public IocContainerHolder(IComponentContext container)
		{
			ToggleManager = container.Resolve<IToggleManager>();
			TenantLogonInfoLoader = container.Resolve<ITenantLogonInfoLoader>();
			IocContainer = container;
		}

		public IComponentContext IocContainer { get; set; }

		public IToggleManager ToggleManager { get; private set; }
		public ITenantLogonInfoLoader TenantLogonInfoLoader { get; private set; }
	}
}