using Teleopti.Ccc.Domain.ApplicationLayer.Events;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers
{
	public class ProjectionChangedEventLightPublisher : 
		IHandleEvent<ScheduleChangedEvent>,
		IRunOnHangfire
	{
		private readonly IEventPublisher _publisher;

		public ProjectionChangedEventLightPublisher(IEventPublisher publisher)
		{
			_publisher = publisher;
		}

		public void Handle(ScheduleChangedEvent @event)
		{
			_publisher.Publish(new ProjectionChangedEventLight());
		}
	}
}