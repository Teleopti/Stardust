using System;
using System.Linq;
using System.Threading;
using log4net;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.RealTimeAdherence.Domain.AgentAdherenceDay;
using Teleopti.Ccc.Domain.RealTimeAdherence.Domain.Events;
using Teleopti.Ccc.Domain.UnitOfWork;

namespace Teleopti.Ccc.Infrastructure.RealTimeAdherence
{
	public interface IRtaEventPublisher : IEventPublisher
	{
	}
	
	[RemoveMeWithToggle(Toggles.RTA_RemoveApprovedOOA_47721)]
	public class SafeRtaEventPublisher : IRtaEventPublisher
	{
		public void Publish(params IEvent[] events)
		{
		}
	}
	
	public class RtaEventPublisher : IRtaEventPublisher
	{
		private readonly IRtaEventStore _store;
		private readonly WithUnitOfWork _unitOfWork;
		private readonly ILog _logger;

		public RtaEventPublisher(IRtaEventStore store, WithUnitOfWork unitOfWork, ILog logger)
		{
			_logger = logger;
			_unitOfWork = unitOfWork;
			_store = store;
		}

		public void Publish(params IEvent[] events)
		{
			var storedEvents = events.OfType<IRtaStoredEvent>().Cast<IEvent>().ToArray();
			if (!storedEvents.Any())
				return;

			var thread = new Thread(() =>
			{
				try
				{
					_unitOfWork.Do(() => storedEvents.ForEach(_store.Add));
				}
				catch (Exception e)
				{
					_logger.Error("Rta event store publishing failed", e);
				}
			});
			thread.Start();
			thread.Join();
		}
	}
}