using System;
using System.Threading;
using System.Threading.Tasks;
using Owin;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.DistributedLock;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.Hangfire;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Web.Core.Startup.Booter;

namespace Teleopti.Ccc.Web.Core.Startup.InitializeApplication
{
	[TaskPriority(2000)]
	public class RequeueHangfireEventsTask : IBootstrapperTask
	{
		private readonly IDistributedLockAcquirer _distributedLockAcquirer;
		private readonly IRequeueHangfireRepository _requeueHangfireRepository;
		private readonly IRequeueFailedHangfireEvents _requeueFailedHangfireEvents;
		private readonly ILoadAllTenants _loadAllTenants;
		private readonly IDataSourceScope _dataSourceScope;
		private readonly ITenantUnitOfWork _tenantUnitOfWork;
		private readonly ICurrentAnalyticsUnitOfWorkFactory _currentAnalyticsUnitOfWorkFactory;

		public RequeueHangfireEventsTask(IDistributedLockAcquirer distributedLockAcquirer, IRequeueHangfireRepository requeueHangfireRepository, IRequeueFailedHangfireEvents requeueFailedHangfireEvents, ILoadAllTenants loadAllTenants, IDataSourceScope dataSourceScope, ITenantUnitOfWork tenantUnitOfWork, ICurrentAnalyticsUnitOfWorkFactory currentAnalyticsUnitOfWorkFactory)
		{
			_distributedLockAcquirer = distributedLockAcquirer;
			_requeueHangfireRepository = requeueHangfireRepository;
			_requeueFailedHangfireEvents = requeueFailedHangfireEvents;
			_loadAllTenants = loadAllTenants;
			_dataSourceScope = dataSourceScope;
			_tenantUnitOfWork = tenantUnitOfWork;
			_currentAnalyticsUnitOfWorkFactory = currentAnalyticsUnitOfWorkFactory;
		}

		public Task Execute(IAppBuilder application)
		{
			return Task.Run(() => onAllTenants());
		}
		
		private void onAllTenants()
		{
			Thread.Sleep(TimeSpan.FromSeconds(20)); // TODO: Remove once License does not need a restart
			using (_tenantUnitOfWork.EnsureUnitOfWorkIsStarted())
				foreach (var tenant in _loadAllTenants.Tenants())
					using (_dataSourceScope.OnThisThreadUse(tenant.Name))
						requeueEvents(tenant.Name);
		}
		
		private void requeueEvents(string tenant)
		{
			// Only allow one web server to run this
			_distributedLockAcquirer.TryLockForTypeOf(this, () =>
			{
				using (var uow = _currentAnalyticsUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
				{
					foreach (var command in _requeueHangfireRepository.GetUnhandledRequeueCommands())
					{
						_requeueFailedHangfireEvents.RequeueFailed(command.EventName, command.HandlerName, tenant);
						_requeueHangfireRepository.MarkAsCompleted(command);
						uow.PersistAll();
					}
				}
			});
		}
	}
}