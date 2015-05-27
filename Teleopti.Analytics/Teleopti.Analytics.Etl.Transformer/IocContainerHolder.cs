using Autofac;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Ccc.Infrastructure.Toggle;

namespace Teleopti.Analytics.Etl.Transformer
{
	public interface IContainerHolder
	{
		IToggleManager ToggleManager { get; }
		ITenantLogonInfoLoader TenantLogonInfoLoader { get; }
	}
	public class IocContainerHolder : IContainerHolder
	{
		//private readonly IContainer _container;

		public IocContainerHolder(IContainer container)
		{
			//_container = container;
			ToggleManager = container.Resolve<IToggleManager>();
			TenantLogonInfoLoader = container.Resolve<ITenantLogonInfoLoader>();
		}

		public IToggleManager ToggleManager { get; private set; }

		public ITenantLogonInfoLoader TenantLogonInfoLoader { get; private set; }
	}
}