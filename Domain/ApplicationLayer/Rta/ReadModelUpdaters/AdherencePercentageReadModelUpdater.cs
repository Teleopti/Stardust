using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters
{
	[DisabledBy(Toggles.RTA_EventPackagesOptimization_43924)]
	public class AdherencePercentageReadModelUpdater : AdherencePercentageReadModelUpdaterImpl,
		IHandleEvent<PersonInAdherenceEvent>,
		IHandleEvent<PersonOutOfAdherenceEvent>,
		IHandleEvent<PersonNeutralAdherenceEvent>,
		IHandleEvent<PersonShiftStartEvent>,
		IHandleEvent<PersonShiftEndEvent>,
		IHandleEvent<PersonDeletedEvent>,
		IRunOnHangfire
	{
		public AdherencePercentageReadModelUpdater(IAdherencePercentageReadModelPersister persister) : base(persister)
		{
		}

		[ReadModelUnitOfWork]
		public virtual void Handle(PersonInAdherenceEvent @event)
		{
			handle(@event);
		}

		[ReadModelUnitOfWork]
		public virtual void Handle(PersonOutOfAdherenceEvent @event)
		{
			handle(@event);
		}

		[ReadModelUnitOfWork]
		public virtual void Handle(PersonShiftStartEvent @event)
		{
			handle(@event);
		}
		
		[ReadModelUnitOfWork]
		public virtual void Handle(PersonShiftEndEvent @event)
		{
			handle(@event);
		}

		[ReadModelUnitOfWork]
		public virtual void Handle(PersonNeutralAdherenceEvent @event)
		{
			handle(@event);
		}

		[ReadModelUnitOfWork]
		public virtual void Handle(PersonDeletedEvent @event)
		{
			handle(@event);
		}
	}

	[EnabledBy(Toggles.RTA_EventPackagesOptimization_43924)]
	public class AdherencePercentageReadModelUpdaterWithPackages : AdherencePercentageReadModelUpdaterImpl, IHandleEvents, IRunOnHangfire
	{
		public AdherencePercentageReadModelUpdaterWithPackages(IAdherencePercentageReadModelPersister persister) : base(persister)
		{
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
	}

	public abstract class AdherencePercentageReadModelUpdaterImpl
	{
		private readonly IAdherencePercentageReadModelPersister _persister;

		protected AdherencePercentageReadModelUpdaterImpl(IAdherencePercentageReadModelPersister persister)
		{
			_persister = persister;
		}
		
		protected void handle(PersonShiftStartEvent @event)
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
		
		protected void handle(PersonInAdherenceEvent @event)
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

		protected void handle(PersonOutOfAdherenceEvent @event)
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
		
		protected void handle(PersonNeutralAdherenceEvent @event)
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

		protected void handle(PersonShiftEndEvent @event)
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

		protected void handle(PersonDeletedEvent @event)
		{
			_persister.Delete(@event.PersonId);
		}

		private void handleEvent(Guid personId, DateOnly? belongsToDate, AdherencePercentageReadModelState readModelState, Action<AdherencePercentageReadModel> mutate)
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