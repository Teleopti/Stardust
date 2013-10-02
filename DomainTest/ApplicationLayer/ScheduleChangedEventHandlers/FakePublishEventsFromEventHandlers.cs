using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.ScheduleChangedEventHandlers
{
	public class FakePublishEventsFromEventHandlers : IPublishEventsFromEventHandlers
	{
		private IEvent _published;

		public void Publish(IEvent @event)
		{
			_published = @event;
		}

		public T Published<T>() where T : class 
		{
			return _published as T;
		}

	}
}