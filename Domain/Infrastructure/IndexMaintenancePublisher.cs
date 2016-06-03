using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Infrastructure.Events;

namespace Teleopti.Ccc.Domain.Infrastructure
{
	public class IndexMaintenancePublisher
	{
		private readonly IRecurringEventPublisher _recurringEventPublisher;

		public IndexMaintenancePublisher(IRecurringEventPublisher recurringEventPublisher)
		{
			_recurringEventPublisher = recurringEventPublisher;
		}

		public void Start()
		{
			_recurringEventPublisher.PublishDaily(new IndexMaintenance());
		}
	}
}