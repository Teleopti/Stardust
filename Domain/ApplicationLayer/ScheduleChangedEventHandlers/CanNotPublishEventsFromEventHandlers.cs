namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers
{
	public class CanNotPublishEventsFromEventHandlers : IPublishEventsFromEventHandlers
	{
		public void Publish(object @event)
		{
			throw new System.NotImplementedException();
		}
	}
}