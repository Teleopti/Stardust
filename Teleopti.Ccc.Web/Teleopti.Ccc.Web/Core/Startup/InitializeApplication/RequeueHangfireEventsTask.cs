using System;
using System.Linq;
using System.Threading.Tasks;
using Owin;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Infrastructure.Events;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Web.Core.Startup.Booter;

namespace Teleopti.Ccc.Web.Core.Startup.InitializeApplication
{
	[TaskPriority(2000)]
	public class RequeueHangfireEventsTask : IBootstrapperTask
	{
		private readonly ILoadAllTenants _loadAllTenants;
		private readonly ITenantUnitOfWork _tenantUnitOfWork;
		private readonly IEventPublisher _eventPublisher;

		public RequeueHangfireEventsTask(ILoadAllTenants loadAllTenants, ITenantUnitOfWork tenantUnitOfWork, IEventPublisher eventPublisher)
		{
			_loadAllTenants = loadAllTenants;
			_tenantUnitOfWork = tenantUnitOfWork;
			_eventPublisher = eventPublisher;
		}

		public Task Execute(IAppBuilder application)
		{
			return Task.Run(() => publishEvent());
		}
		
		private void publishEvent()
		{
			using (_tenantUnitOfWork.EnsureUnitOfWorkIsStarted())
			{
				var tenants = _loadAllTenants.Tenants();
				_eventPublisher.Publish(new RequeueHangfireEvent
				{
					Tenants = tenants.Select(tenant => tenant.Name).ToList()
				});
			}
		}
	}
}