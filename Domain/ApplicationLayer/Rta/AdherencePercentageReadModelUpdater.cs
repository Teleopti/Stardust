using System;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Client;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta
{
	public class AdherencePercentageReadModelUpdater :
		IHandleEvent<PersonInAdherenceEvent>,
		IHandleEvent<PersonOutOfAdherenceEvent>,
		IHandleEvent<PersonShiftEndEvent>
	{
		private readonly IAdherencePercentageReadModelPersister _persister;
		private readonly ILiteTransactionSyncronization _transactionSync;
		private readonly IMessageSender _messageSender;

		public AdherencePercentageReadModelUpdater(IAdherencePercentageReadModelPersister persister, ILiteTransactionSyncronization transactionSync, IMessageSender messageSender)
		{
			_persister = persister;
			_transactionSync = transactionSync;
			_messageSender = messageSender;
		}

		[ReadModelUnitOfWork]
		public virtual void Handle(PersonInAdherenceEvent @event)
		{
			handleEvent(@event.PersonId, @event.Timestamp, m => m.IsLastTimeInAdherence = true);
			sendMessageAfterReadModelUpdated(@event.Datasource, @event.BusinessUnitId);
		}

		[ReadModelUnitOfWork]
		public virtual void Handle(PersonOutOfAdherenceEvent @event)
		{
			handleEvent(@event.PersonId, @event.Timestamp, m => m.IsLastTimeInAdherence = false);
			sendMessageAfterReadModelUpdated(@event.Datasource, @event.BusinessUnitId);
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
			sendMessageAfterReadModelUpdated(@event.Datasource, @event.BusinessUnitId);
		}

		private void sendMessageAfterReadModelUpdated(string datasource, Guid businessUnitId)
		{
			_transactionSync.OnSuccessfulTransaction(
				() => _messageSender.Send(new Interfaces.MessageBroker.Notification
				{
					DataSource = datasource,
					BusinessUnitId = businessUnitId.ToString(),
					DomainType = typeof(AdherencePercentageReadModelUpdatedMessage).Name
				}));
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