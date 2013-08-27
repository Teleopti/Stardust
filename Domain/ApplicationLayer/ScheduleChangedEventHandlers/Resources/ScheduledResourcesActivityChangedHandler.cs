using System.Xml;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.Resources
{
	public class ScheduledResourcesActivityChangedHandler : IHandleEvent<ActivityChangedEvent>
	{
		private readonly IScheduledResourcesReadModelStorage _scheduledResourcesReadModelStorage;

		public ScheduledResourcesActivityChangedHandler(IScheduledResourcesReadModelStorage scheduledResourcesReadModelStorage)
		{
			_scheduledResourcesReadModelStorage = scheduledResourcesReadModelStorage;
		}

		public void Handle(ActivityChangedEvent @event)
		{
			if (@event.Property == "RequiresSeat")
			{
				_scheduledResourcesReadModelStorage.ActivityUpdated(@event.ActivityId, XmlConvert.ToBoolean(@event.NewValue));
			}
		}
	}
}