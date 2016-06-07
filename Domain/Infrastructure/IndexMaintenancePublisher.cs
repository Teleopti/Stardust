using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Infrastructure.Events;

namespace Teleopti.Ccc.Domain.Infrastructure
{
	public class IndexMaintenancePublisher
	{
		private readonly AllTenantRecurringEventPublisher _allTenantRecurringEventPublisher;

		public IndexMaintenancePublisher(AllTenantRecurringEventPublisher allTenantRecurringEventPublisher)
		{
			_allTenantRecurringEventPublisher = allTenantRecurringEventPublisher;
		}

		public void Start()
		{
			_allTenantRecurringEventPublisher.PublishDaily(new IndexMaintenanceHangfireEvent());
		}
	}
}