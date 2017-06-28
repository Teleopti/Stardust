using System;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.DistributedLock;
using Teleopti.Ccc.Domain.Infrastructure.Events;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Domain.Infrastructure
{
	public class RequeueHangfireEventHandler : 
		IHandleEvent<RequeueHangfireEvent>,
		IRunOnHangfire
	{
		private readonly IDistributedLockAcquirer _distributedLockAcquirer;
		private readonly IRequeueHangfireRepository _requeueHangfireRepository;
		private readonly IManageFailedHangfireEvents _manageFailedHangfireEvents;
		private readonly IDataSourceScope _dataSourceScope;
		private readonly ICurrentAnalyticsUnitOfWorkFactory _currentAnalyticsUnitOfWorkFactory;

		public RequeueHangfireEventHandler(IDistributedLockAcquirer distributedLockAcquirer, IRequeueHangfireRepository requeueHangfireRepository, IManageFailedHangfireEvents manageFailedHangfireEvents, IDataSourceScope dataSourceScope, ICurrentAnalyticsUnitOfWorkFactory currentAnalyticsUnitOfWorkFactory)
		{
			_distributedLockAcquirer = distributedLockAcquirer;
			_requeueHangfireRepository = requeueHangfireRepository;
			_manageFailedHangfireEvents = manageFailedHangfireEvents;
			_dataSourceScope = dataSourceScope;
			_currentAnalyticsUnitOfWorkFactory = currentAnalyticsUnitOfWorkFactory;
		}

		public void Handle(RequeueHangfireEvent @event)
		{
			foreach (var tenant in @event.Tenants)
			using (_dataSourceScope.OnThisThreadUse(tenant))
				requeueEvents(tenant);
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
						_manageFailedHangfireEvents.RequeueFailed(command.EventName, command.HandlerName, tenant);
						_requeueHangfireRepository.MarkAsCompleted(command);
						uow.PersistAll();
					}
				}
			});
		}
	}

	public interface IManageFailedHangfireEvents
	{
		void RequeueFailed(string eventName, string handlerName, string tenant);
		void DeleteFailed(string eventName, string handlerName, string tenant);
	}
}