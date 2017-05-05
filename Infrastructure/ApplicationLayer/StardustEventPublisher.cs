using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

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
			_resolver.JobsFor<IRunOnStardust>(events)
				.Select(x => x.Event)
				.Where(x => x != null)
				.Distinct()
				.ForEach(x => _sender.Send(x));
		}
	}
}