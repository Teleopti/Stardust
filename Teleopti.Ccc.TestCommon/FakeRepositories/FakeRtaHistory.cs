using System;
using System.Drawing;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.RealTimeAdherence.Domain.AgentAdherenceDay;
using Teleopti.Ccc.Domain.RealTimeAdherence.Domain.Events;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeRtaHistory
	{
		private readonly IRtaEventStore _store;

		public FakeRtaHistory(IRtaEventStore store)
		{
			_store = store;
		}

		public FakeRtaHistory StateChanged(Guid personId, string time) =>
			StateChanged(personId, time, null, null, null, null, null, null);

		public FakeRtaHistory StateChanged(Guid personId, string time, string state, string activity, Color? activityColor, string rule, Color? ruleColor, Adherence? adherence)
		{
			_store.Add(new PersonStateChangedEvent
			{
				PersonId = personId,
				BelongsToDate = time.Date(),
				Timestamp = time.Utc(),
				StateName = state,
				ActivityName = activity,
				ActivityColor = activityColor?.ToArgb(),
				RuleName = rule,
				RuleColor = ruleColor?.ToArgb(),
				Adherence = adherence == null ? null : (EventAdherence?) Enum.Parse(typeof(EventAdherence), adherence.ToString()),
			});
			return this;
		}

		public FakeRtaHistory RuleChanged(Guid personId, string time, string rule) =>
			RuleChanged(personId, time, null, null, null, rule, null, null);

		public FakeRtaHistory RuleChanged(Guid personId, string time, string state, string activity, Color? activityColor, string rule, Color? ruleColor, Adherence? adherence)
		{
			_store.Add(new PersonRuleChangedEvent
			{
				PersonId = personId,
				BelongsToDate = time.Date(),
				Timestamp = time.Utc(),
				StateName = state,
				ActivityName = activity,
				ActivityColor = activityColor?.ToArgb(),
				RuleName = rule,
				RuleColor = ruleColor?.ToArgb(),
				Adherence = adherence == null ? null : (EventAdherence?) Enum.Parse(typeof(EventAdherence), adherence.ToString()),
			});
			return this;
		}

		public FakeRtaHistory BackFromLateForWork(Guid personId, string shiftStart, string time) =>
			BackFromLateForWork(personId, shiftStart, time, null, null, null, null, null, null);

		public FakeRtaHistory BackFromLateForWork(Guid personId, string shiftStart, string time, string state, string activity, Color? activityColor, string rule, Color? ruleColor, Adherence? adherence)
		{
			_store.Add(new PersonArrivalAfterLateForWorkEvent
			{
				PersonId = personId,
				Timestamp = time.Utc(),
				ShiftStart = shiftStart.Utc(),
				StateName = state,
				ActivityName = activity,
				ActivityColor = activityColor?.ToArgb(),
				RuleName = rule,
				RuleColor = ruleColor?.ToArgb(),
				Adherence = adherence == null ? null : (EventAdherence?) Enum.Parse(typeof(EventAdherence), adherence.ToString())
			});
			return this;
		}
	}
}