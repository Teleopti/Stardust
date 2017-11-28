using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters
{
	public static class AdherencePercentageReadModelUpdaterExtensions
	{
		public static void Handle(this AdherencePercentageReadModelUpdater instance, PersonInAdherenceEvent @event)
		{
			instance.Handle(@event.AsArray());
		}

		public static void Handle(this AdherencePercentageReadModelUpdater instance, PersonOutOfAdherenceEvent @event)
		{
			instance.Handle(@event.AsArray());
		}

		public static void Handle(this AdherencePercentageReadModelUpdater instance, PersonShiftStartEvent @event)
		{
			instance.Handle(@event.AsArray());
		}

		public static void Handle(this AdherencePercentageReadModelUpdater instance, PersonShiftEndEvent @event)
		{
			instance.Handle(@event.AsArray());
		}

		public static void Handle(this AdherencePercentageReadModelUpdater instance, PersonNeutralAdherenceEvent @event)
		{
			instance.Handle(@event.AsArray());
		}

		public static void Handle(this AdherencePercentageReadModelUpdater instance, PersonDeletedEvent @event)
		{
			instance.Handle(@event.AsArray());
		}
	}

	public class AdherencePercentageReadModelUpdater :
		IHandleEvents,
		IRunInSync
	{
		private readonly IAdherencePercentageReadModelPersister _persister;

		public AdherencePercentageReadModelUpdater(IAdherencePercentageReadModelPersister persister)
		{
			_persister = persister;
		}

		public virtual void Subscribe(SubscriptionRegistrator registrator)
		{
			registrator.SubscribeTo<PersonInAdherenceEvent>();
			registrator.SubscribeTo<PersonOutOfAdherenceEvent>();
			registrator.SubscribeTo<PersonNeutralAdherenceEvent>();
			registrator.SubscribeTo<PersonShiftStartEvent>();
			registrator.SubscribeTo<PersonShiftEndEvent>();
			registrator.SubscribeTo<PersonDeletedEvent>();
		}

		[ReadModelUnitOfWork]
		public virtual void Handle(IEnumerable<IEvent> events)
		{
			events.ForEach(e => handle((dynamic) e));
		}

		private void handle(PersonShiftStartEvent @event)
		{
			handleEvent(
				@event.PersonId,
				@event.BelongsToDate,
				new AdherencePercentageReadModelState
				{
					Timestamp = @event.ShiftStartTime,
					ShiftStarted = true
				},
				m =>
				{
					m.ShiftStartTime = @event.ShiftStartTime;
					m.ShiftEndTime = @event.ShiftEndTime;
				});
		}

		private void handle(PersonInAdherenceEvent @event)
		{
			handleEvent(
				@event.PersonId,
				@event.BelongsToDate,
				new AdherencePercentageReadModelState
				{
					Timestamp = @event.Timestamp,
					InAdherence = true
				},
				m => m.IsLastTimeInAdherence = true);
		}

		private void handle(PersonOutOfAdherenceEvent @event)
		{
			handleEvent(
				@event.PersonId,
				@event.BelongsToDate,
				new AdherencePercentageReadModelState
				{
					Timestamp = @event.Timestamp,
					InAdherence = false
				},
				m => m.IsLastTimeInAdherence = false);
		}

		private void handle(PersonNeutralAdherenceEvent @event)
		{
			handleEvent(
				@event.PersonId,
				@event.BelongsToDate,
				new AdherencePercentageReadModelState
				{
					Timestamp = @event.Timestamp,
					InAdherence = null
				},
				m => m.IsLastTimeInAdherence = null);
		}

		private void handle(PersonShiftEndEvent @event)
		{
			handleEvent(
				@event.PersonId,
				@event.BelongsToDate,
				new AdherencePercentageReadModelState
				{
					Timestamp = @event.ShiftEndTime,
					ShiftEnded = true
				},
				m => m.ShiftHasEnded = true);
		}

		private void handle(PersonDeletedEvent @event)
		{
			_persister.Delete(@event.PersonId);
		}

		private void handleEvent(Guid personId, DateOnly? belongsToDate, AdherencePercentageReadModelState readModelState,
			Action<AdherencePercentageReadModel> mutate)
		{
			var date = belongsToDate.HasValue ? belongsToDate.Value : new DateOnly(readModelState.Timestamp);
			var model = _persister.Get(date, personId);
			if (model == null)
			{
				model = new AdherencePercentageReadModel
				{
					Date = date.Date,
					PersonId = personId,
					LastTimestamp = readModelState.Timestamp,
					State = new[] {readModelState}
				};
			}
			else
			{
				model.State = model.State.Concat(new[] {readModelState});
				if (readModelState.Timestamp - model.LastTimestamp.GetValueOrDefault() > TimeSpan.Zero)
					model.LastTimestamp = readModelState.Timestamp;
				calculate(model);
			}
			mutate(model);
			_persister.Persist(model);
		}

		private static void calculate(AdherencePercentageReadModel model)
		{
			AdherencePercentageReadModelState previous = null;
			resetAdherence(model);
			model.State = model.State.OrderBy(x => x.Timestamp).ToArray();
			foreach (var current in model.State)
			{
				if (previous != null)
				{
					if (current.ShiftStarted.GetValueOrDefault(false))
					{
						resetAdherence(model);
						current.InAdherence = previous.InAdherence;
					}
					else if (previous.InAdherence.HasValue)
					{
						var timeDifferenceBetweenCurrentAndPrevious = current.Timestamp - previous.Timestamp;
						if (previous.InAdherence.Value)
							model.TimeInAdherence += timeDifferenceBetweenCurrentAndPrevious;
						if (!previous.InAdherence.Value)
							model.TimeOutOfAdherence += timeDifferenceBetweenCurrentAndPrevious;
					}
				}
				if (current.ShiftEnded.GetValueOrDefault(false))
					break;
				previous = current;
			}
		}

		private static void resetAdherence(AdherencePercentageReadModel model)
		{
			model.TimeInAdherence = TimeSpan.Zero;
			model.TimeOutOfAdherence = TimeSpan.Zero;
		}
	}
}