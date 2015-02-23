using System.Collections.Generic;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta
{
	public class RtaDecoratingEventPublisher : IRtaDecoratingEventPublisher
	{
		private readonly IEventPopulatingPublisher _publisher;
		private readonly IEnumerable<IRtaEventDecorator> _decorators;

		public RtaDecoratingEventPublisher(IEventPopulatingPublisher publisher, IEnumerable<IRtaEventDecorator> decorators)
		{
			_publisher = publisher;
			_decorators = decorators;
		}

		public void Publish(StateInfo info, IEvent @event)
		{
			_decorators.ForEach(d => d.Decorate(info, @event));
			_publisher.Publish(@event);
		}
	}
}