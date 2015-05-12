using Autofac;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;
using Teleopti.Ccc.Infrastructure.Toggle;

namespace Teleopti.Analytics.Etl.Transformer
{
	public interface IContainerHolder
	{
		IToggleManager ToggleManager { get; }
		ITenantLogonDataManager TenantLogonDataManager { get; }
	}
	public class IocContainerHolder : IContainerHolder
	{
		//private readonly IContainer _container;

		public IocContainerHolder(IContainer container)
		{
			//_container = container;
			ToggleManager = container.Resolve<IToggleManager>();
			TenantLogonDataManager = container.Resolve<ITenantLogonDataManager>();
		}

		public IToggleManager ToggleManager { get; private set; }

		public ITenantLogonDataManager TenantLogonDataManager { get; private set; }
	}
}