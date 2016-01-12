using Autofac;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Ccc.Infrastructure.Toggle;

namespace Teleopti.Analytics.Etl.Common.Transformer
{
	public interface IContainerHolder
	{
		IToggleManager ToggleManager { get; }
		ITenantLogonInfoLoader TenantLogonInfoLoader { get; }
	}
	public class IocContainerHolder : IContainerHolder
	{
		public IocContainerHolder(IComponentContext container)
		{
			ToggleManager = container.Resolve<IToggleManager>();
			TenantLogonInfoLoader = container.Resolve<ITenantLogonInfoLoader>();
		}

		public IToggleManager ToggleManager { get; private set; }

		public ITenantLogonInfoLoader TenantLogonInfoLoader { get; private set; }
	}
}