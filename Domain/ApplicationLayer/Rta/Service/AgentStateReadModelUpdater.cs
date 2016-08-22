using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public interface IAgentStateReadModelUpdater
	{
		void Update(Context context, IEnumerable<IEvent> events);
	}

	public class AgentStateReadModelUpdaterWithOutOfAdherences : AgentStateReadModelUpdater
	{
		private readonly INow _now;

		public AgentStateReadModelUpdaterWithOutOfAdherences(IAgentStateReadModelPersister persister, INow now) : base(persister)
		{
			_now = now;
		}

		protected override AgentStateReadModel LoadModel(Context context)
		{
			return _persister.Get(context.PersonId) ?? base.LoadModel(context);
		}

		protected override void BeforePersist(AgentStateReadModel model, IEnumerable<IEvent> events)
		{
			applyEvents(model, events);
		}

		private void applyEvents(AgentStateReadModel model, IEnumerable<IEvent> events)
		{
			foreach (var @event in events)
			{
				if (@event is PersonOutOfAdherenceEvent)
					handle(model, @event as PersonOutOfAdherenceEvent);
				else if (@event is PersonInAdherenceEvent)
					handle(model, @event as PersonInAdherenceEvent);
				else if (@event is PersonNeutralAdherenceEvent)
					handle(model, @event as PersonNeutralAdherenceEvent);
			}

			if (model.OutOfAdherences != null)
				model.OutOfAdherences = model.OutOfAdherences
					.Where(x => x.EndTime == null || x.EndTime > _now.UtcDateTime().AddHours(-1));
		}

		private static void handle(AgentStateReadModel model, PersonNeutralAdherenceEvent @event)
		{
			var last = model.OutOfAdherences.EmptyIfNull().LastOrDefault();
			if (last != null && last.EndTime == null)
				last.EndTime = @event.Timestamp;
		}

		private static void handle(AgentStateReadModel model, PersonInAdherenceEvent @event)
		{
			var last = model.OutOfAdherences.EmptyIfNull().LastOrDefault();
			if (last != null && last.EndTime == null)
				last.EndTime = @event.Timestamp;
		}

		private static void handle(AgentStateReadModel model, PersonOutOfAdherenceEvent @event)
		{
			model.OutOfAdherences =
				model.OutOfAdherences.EmptyIfNull().Append(new AgentStateOutOfAdherenceReadModel
				{
					StartTime = @event.Timestamp,
					EndTime = null
				}).ToArray();
		}
	}

	public class AgentStateReadModelUpdater : IAgentStateReadModelUpdater
	{
		protected readonly IAgentStateReadModelPersister _persister;

		public AgentStateReadModelUpdater(IAgentStateReadModelPersister persister)
		{
			_persister = persister;
		}

		protected virtual AgentStateReadModel LoadModel(Context context)
		{
			return new AgentStateReadModel();
		}

		protected virtual void BeforePersist(AgentStateReadModel model, IEnumerable<IEvent> events)
		{
		}

		public void Update(Context context, IEnumerable<IEvent> events)
		{
			var model = LoadModel(context);

			model.ReceivedTime = context.CurrentTime;
			model.PersonId = context.PersonId;
			model.BusinessUnitId = context.BusinessUnitId;
			model.SiteId = context.SiteId;
			model.TeamId = context.TeamId;

			model.Activity = context.Schedule.CurrentActivityName();
			model.NextActivity = context.Schedule.NextActivityName();
			model.NextActivityStartTime = context.Schedule.NextActivityStartTime();

			model.StateCode = context.StateCode;
			model.StateName = context.State.StateGroupName();
			model.StateStartTime = context.StateStartTime;

			model.RuleName = context.State.RuleName();
			model.RuleStartTime = context.RuleStartTime;
			model.RuleColor = context.State.RuleDisplayColor();
			model.StaffingEffect = context.State.StaffingEffect();

			model.IsRuleAlarm = context.IsAlarm;
			model.AlarmStartTime = context.AlarmStartTime;
			model.AlarmColor = context.State.AlarmColor();
			model.Shift = context.Schedule.ActivitiesInTimeWindow()
				.Select(a => new AgentStateActivityReadModel
				{
					Color = a.DisplayColor,
					StartTime = a.StartDateTime,
					EndTime = a.EndDateTime,
					Name = a.Name
				});

			BeforePersist(model, events);

			_persister.Persist(model);
		}

	}

	public class AgentStateReadModelCleaner :
		IRunOnHangfire,
		IHandleEvent<PersonDeletedEvent>,
		IHandleEvent<PersonAssociationChangedEvent>
	{
		private readonly IAgentStateReadModelPersister _persister;

		public AgentStateReadModelCleaner(IAgentStateReadModelPersister persister)
		{
			_persister = persister;
		}

		[UnitOfWork]
		public virtual void Handle(PersonDeletedEvent @event)
		{
			_persister.Delete(@event.PersonId);
		}

		[UnitOfWork]
		public virtual void Handle(PersonAssociationChangedEvent @event)
		{
			if (@event.TeamId.HasValue)
			{
				var existing = _persister.Get(@event.PersonId);
				if (existing == null)
					return;

				existing.TeamId = @event.TeamId;
				existing.SiteId = @event.SiteId;
				_persister.Persist(existing);
			}
			else
			{
				_persister.Delete(@event.PersonId);
			}
		}

	}

}