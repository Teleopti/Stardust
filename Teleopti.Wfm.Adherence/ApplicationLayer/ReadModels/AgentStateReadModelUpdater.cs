using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Wfm.Adherence.Domain.Events;
using Teleopti.Wfm.Adherence.Domain.Service;
using Teleopti.Wfm.Adherence.States;

namespace Teleopti.Wfm.Adherence.ApplicationLayer.ReadModels
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

	public class AgentStateReadModelUpdater :
		IHandleEvents,
		IRunInSync
	{
		private readonly IAgentStateReadModelPersister _persister;
		private readonly INow _now;

		public AgentStateReadModelUpdater(IAgentStateReadModelPersister persister, INow now)
		{
			_persister = persister;
			_now = now;
		}

		public void Subscribe(SubscriptionRegistrator registrator)
		{
			registrator.SubscribeTo<AgentStateChangedEvent>();
			registrator.SubscribeTo<PersonOutOfAdherenceEvent>();
			registrator.SubscribeTo<PersonInAdherenceEvent>();
			registrator.SubscribeTo<PersonNeutralAdherenceEvent>();
			registrator.SubscribeTo<PersonStateChangedEvent>();
			registrator.SubscribeTo<PersonRuleChangedEvent>();
		}

		[UnitOfWork]
		public virtual void Handle(IEnumerable<IEvent> events)
		{
			events.GroupBy(x => ((dynamic) x).PersonId as Guid?)
				.ForEach(eventsForPerson =>
				{
					var model = _persister.Load(eventsForPerson.Key.Value);
					if (model == null)
						return;

					eventsForPerson.ForEach(e => handle(model, (dynamic) e));

					if (model.OutOfAdherences != null)
						model.OutOfAdherences = model.OutOfAdherences
								.Where(x => x.EndTime == null || x.EndTime > _now.UtcDateTime().AddHours(-1))
								.ToArray()
							;

					_persister.UpdateState(model);
				});
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

		private static void handle(AgentStateReadModel model, PersonOutOfAdherenceEvent @event)
		{
			var outOfAdherences = model.OutOfAdherences.EmptyIfNull();
			var last = outOfAdherences.LastOrDefault();
			if (last != null && last.EndTime == null)
				return;
			model.OutOfAdherences = outOfAdherences
				.Append(new AgentStateOutOfAdherenceReadModel
				{
					StartTime = @event.Timestamp,
					EndTime = null
				}).ToArray();
			model.OutOfAdherenceStartTime = @event.Timestamp;
		}

		private static void handle(AgentStateReadModel model, PersonNeutralAdherenceEvent @event) => endOutOfAdherence(model, @event.Timestamp);
		private static void handle(AgentStateReadModel model, PersonInAdherenceEvent @event) => endOutOfAdherence(model, @event.Timestamp);

		private static void endOutOfAdherence(AgentStateReadModel model, DateTime time)
		{
			var last = model.OutOfAdherences.EmptyIfNull().LastOrDefault();
			if (last != null && last.EndTime == null)
				last.EndTime = time;
			model.OutOfAdherenceStartTime = null;
		}
	}
}