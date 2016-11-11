using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public interface IAgentStateReadModelUpdater
	{
		void Update(Context context, IEnumerable<IEvent> events, DeadLockVictim deadLockVictim);
	}

	public class AgentStateReadModelUpdater : IAgentStateReadModelUpdater
	{
		private readonly IAgentStateReadModelPersister _persister;
		private readonly INow _now;

		public AgentStateReadModelUpdater(IAgentStateReadModelPersister persister, INow now)
		{
			_persister = persister;
			_now = now;
		}

		public void Update(Context context, IEnumerable<IEvent> events, DeadLockVictim deadLockVictim)
		{
			var model = _persister.Load(context.PersonId) ?? new AgentStateReadModel();
			if (model.IsDeleted) return;

			model.ReceivedTime = context.CurrentTime;
			model.PersonId = context.PersonId;

			model.Activity = context.Schedule.CurrentActivityName();
			model.NextActivity = context.Schedule.NextActivityName();
			model.NextActivityStartTime = context.Schedule.NextActivityStartTime();

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
				}).ToArray();

			applyEvents(model, events);

			_persister.Persist(model, deadLockVictim);
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
				else if (@event is PersonStateChangedEvent)
					handle(model, @event as PersonStateChangedEvent);
			}

			if (model.OutOfAdherences != null)
				model.OutOfAdherences = model.OutOfAdherences
					.Where(x => x.EndTime == null || x.EndTime > _now.UtcDateTime().AddHours(-1))
					.ToArray()
					;
		}
		
		private static void handle(AgentStateReadModel model, PersonStateChangedEvent @event)
		{
			model.StateCode = @event.StateCode;
			model.StateName = @event.StateGroupName;
			model.StateGroupId = @event.StateGroupId;
			model.StateStartTime = @event.Timestamp;
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
			var outOfAdherences = model.OutOfAdherences.EmptyIfNull();
			var last = outOfAdherences.LastOrDefault();
			if (last == null || last.EndTime != null)
				model.OutOfAdherences = outOfAdherences
					.Append(new AgentStateOutOfAdherenceReadModel
					{
						StartTime = @event.Timestamp,
						EndTime = null
					}).ToArray();
		}
	}
}