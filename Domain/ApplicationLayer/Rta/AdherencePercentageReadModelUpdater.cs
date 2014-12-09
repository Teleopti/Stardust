using System;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Client.Composite;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta
{
	public class AdherencePercentageReadModelUpdater :
		IHandleEvent<PersonInAdherenceEvent>,
		IHandleEvent<PersonOutOfAdherenceEvent>,
		IHandleEvent<PersonShiftEndEvent>
	{
		private readonly IAdherencePercentageReadModelPersister _persister;
		private readonly IEventSyncronization _eventSyncronization;
		private readonly IMessageCreator _messageSender;

		public AdherencePercentageReadModelUpdater(IAdherencePercentageReadModelPersister persister, IEventSyncronization eventSyncronization, IMessageCreator messageSender)
		{
			_persister = persister;
			_eventSyncronization = eventSyncronization;
			_messageSender = messageSender;
		}

		[ReadModelUnitOfWork]
		public virtual void Handle(PersonInAdherenceEvent @event)
		{
			handleEvent(@event.PersonId, @event.Timestamp, m => m.IsLastTimeInAdherence = true);
			sendMessageAfterReadModelUpdated(@event.Datasource, @event.BusinessUnitId, @event.Timestamp);
		}

		[ReadModelUnitOfWork]
		public virtual void Handle(PersonOutOfAdherenceEvent @event)
		{
			handleEvent(@event.PersonId, @event.Timestamp, m => m.IsLastTimeInAdherence = false);
			sendMessageAfterReadModelUpdated(@event.Datasource, @event.BusinessUnitId, @event.Timestamp);
		}

		[ReadModelUnitOfWork]
		public virtual void Handle(PersonShiftEndEvent @event)
		{
			handleEvent(@event.PersonId, @event.ShiftEndTime, m =>
			{
				m.ShiftHasEnded = true;
				m.LastTimestamp = null;
				m.IsLastTimeInAdherence = null;
			});
			sendMessageAfterReadModelUpdated(@event.Datasource, @event.BusinessUnitId, @event.ShiftEndTime);
		}
		
		private void sendMessageAfterReadModelUpdated(string datasource, Guid businessUnitId, DateTime timestamp)
		{
			_eventSyncronization.WhenDone(
				() => _messageSender.Send(datasource, businessUnitId, timestamp, timestamp, Guid.Empty,
					Guid.Empty, typeof(ReadModelUpdatedMessage), DomainUpdateType.NotApplicable, null));
		}
		
		private void handleEvent(Guid personId, DateTime time, Action<AdherencePercentageReadModel> mutate)
		{
			var model = _persister.Get(new DateOnly(time), personId);
			if (model == null)
			{
				model = new AdherencePercentageReadModel
				{
					Date = new DateOnly(time),
					PersonId = personId,
					LastTimestamp = time
				};
			}
			else
			{
				if (model.ShiftHasEnded)
					return;
				incrementTime(model, time);
			}
			mutate(model);
			_persister.Persist(model);
		}

		private static void incrementTime(AdherencePercentageReadModel model, DateTime time)
		{
			if (model.IsLastTimeInAdherence.Value)
				model.TimeInAdherence += (time - model.LastTimestamp.Value);
			else
				model.TimeOutOfAdherence += time - model.LastTimestamp.Value;
			model.LastTimestamp = time;
		}

	}
}