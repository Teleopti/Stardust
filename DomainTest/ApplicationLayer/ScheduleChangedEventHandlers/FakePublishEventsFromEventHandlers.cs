using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.ScheduleChangedEventHandlers
{
	public class FakePublishEventsFromEventHandlers : IEventPublisher
	{
		private IEvent _published;

		public void Publish(params IEvent[] events)
		{
			_published = events.Single();
		}

		public T Published<T>() where T : class 
		{
			return _published as T;
		}

	}
}