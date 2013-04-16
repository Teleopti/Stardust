using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers
{
	public class CannotPublishEventsFromEventHandlers : IPublishEventsFromEventHandlers
	{
		public void Publish(IEvent @event)
		{
			throw new System.NotImplementedException();
		}
	}
}