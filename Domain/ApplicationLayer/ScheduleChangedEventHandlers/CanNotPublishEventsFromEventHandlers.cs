namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers
{
	public class CannotPublishEventsFromEventHandlers : IPublishEventsFromEventHandlers
	{
		public void Publish(object @event)
		{
			throw new System.NotImplementedException();
		}
	}
}