using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.Resources
{
	public class DisabledScheduledResourcesReadModelUpdater :
		IScheduledResourcesReadModelUpdater
	{
		public void Update(string dataSource, Guid businessUnitId, Action<IScheduledResourcesReadModelUpdaterActions> action)
		{
			
		}
	}
}