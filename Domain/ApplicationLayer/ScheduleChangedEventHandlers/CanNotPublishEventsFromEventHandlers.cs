using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers
{
	public class CannotPublishEventsFromEventHandlers : IPublishEventsFromEventHandlers
	{
		public void Publish(params IEvent[] events)
		{
			throw new System.NotImplementedException();
		}
	}
}