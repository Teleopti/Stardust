using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	// special event never ment to be used in a queue
	public class AgentStateChangedEvent : IEvent
	{
		public Guid PersonId { get; set; }
		public DateTime Time { get; set; }

		public string CurrentActivityName { get; set; }
		public string NextActivityName { get; set; }
		public DateTime? NextActivityStartTime { get; set; }

		public IEnumerable<ScheduledActivity> ActivitiesInTimeWindow { get; set; }

	}

	public interface IAgentStateReadModelUpdater
	{
		void Handle(AgentStateReadModelUpdaterEventPackage @event);
	}

	public class AgentStateReadModelUpdaterEventPackage : IEvent
	{
		public Guid PersonId { get; set; }
		public IEnumerable<IEvent> Events { get; set; }
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

		public void Handle(AgentStateReadModelUpdaterEventPackage @event)
		{
			var model = _persister.Load(@event.PersonId) ?? new AgentStateReadModel();
			if (model.IsDeleted) return;
			applyEvents(model, @event.Events);
			_persister.Persist(model);
		}
		
		private void applyEvents(AgentStateReadModel model, IEnumerable<IEvent> events)
		{
			foreach (var @event in events)
			{
				if (@event is AgentStateChangedEvent)
					handle(model, @event as AgentStateChangedEvent);
				if (@event is PersonOutOfAdherenceEvent)
					handle(model, @event as PersonOutOfAdherenceEvent);
				else if (@event is PersonInAdherenceEvent)
					handle(model, @event as PersonInAdherenceEvent);
				else if (@event is PersonNeutralAdherenceEvent)
					handle(model, @event as PersonNeutralAdherenceEvent);
				else if (@event is PersonStateChangedEvent)
					handle(model, @event as PersonStateChangedEvent);
				else if (@event is PersonRuleChangedEvent)
					handle(model, @event as PersonRuleChangedEvent);
			}

			if (model.OutOfAdherences != null)
				model.OutOfAdherences = model.OutOfAdherences
					.Where(x => x.EndTime == null || x.EndTime > _now.UtcDateTime().AddHours(-1))
					.ToArray()
					;
		}
		
		private static void handle(AgentStateReadModel model, AgentStateChangedEvent @event)
		{
			model.ReceivedTime = @event.Time;
			model.PersonId = @event.PersonId;

			model.Activity = @event.CurrentActivityName;
			model.NextActivity = @event.NextActivityName;
			model.NextActivityStartTime = @event.NextActivityStartTime;

			model.Shift = @event.ActivitiesInTimeWindow
				.Select(a => new AgentStateActivityReadModel
				{
					Color = a.DisplayColor,
					StartTime = a.StartDateTime,
					EndTime = a.EndDateTime,
					Name = a.Name
				})
				.ToArray();
		}

		private static void handle(AgentStateReadModel model, PersonRuleChangedEvent @event)
		{
			model.RuleName = @event.RuleName;
			model.RuleStartTime = @event.Timestamp;
			model.RuleColor = @event.RuleColor;

			model.StaffingEffect = @event.StaffingEffect;
			model.IsRuleAlarm = @event.IsAlarm;
			model.AlarmStartTime = @event.AlarmStartTime;
			model.AlarmColor = @event.AlarmColor;
		}

		private static void handle(AgentStateReadModel model, PersonStateChangedEvent @event)
		{
			model.StateName = @event.StateName;
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