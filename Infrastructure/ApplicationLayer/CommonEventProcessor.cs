using System;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Logon.Aspects;
using Teleopti.Ccc.Domain.MessageBroker;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
	public class CommonEventProcessor
	{
		private readonly ResolveEventHandlers _resolver;
		private readonly ITrackingMessageSender _trackingMessageSender;

		public CommonEventProcessor(
			ResolveEventHandlers resolver,
			ITrackingMessageSender trackingMessageSender)
		{
			_resolver = resolver;
			_trackingMessageSender = trackingMessageSender;
		}

		[TenantScope]
		public virtual void Process(string tenant, IEvent @event, object handler)
		{
			Process(@event, handler);
		}

		public virtual void Process(IEvent @event, object handler)
		{
			var commandIdentifier = @event as ICommandIdentifier;
			try
			{
				new SyncPublishTo(_resolver, handler).Publish(@event);
			}
			catch (Exception)
			{
				if (_trackingMessageSender == null) throw;
				if (commandIdentifier == null) throw;
				if (commandIdentifier.CommandId != Guid.Empty)
					_trackingMessageSender.SendTrackingMessage(@event, new TrackingMessage
					{
						Status = TrackingMessageStatus.Failed,
						TrackId = commandIdentifier.CommandId
					});
				throw;
			}
		}

	}
}