using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
	public class StardustEventPublisher : IEventPublisher
	{
		private readonly ResolveEventHandlers _resolver;
		private readonly IStardustSender _sender;

		public StardustEventPublisher(
			ResolveEventHandlers resolver,
			IStardustSender sender)
		{
			_resolver = resolver;
			_sender = sender;
		}

		public void Publish(params IEvent[] events)
		{
			events.Where(e => _resolver.HandlerTypesFor<IRunOnStardust>(e).Any()).ForEach(b => _sender.Send(b));
		}
	}
}