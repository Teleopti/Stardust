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

		public FakeRtaHistory StateChanged(Guid personId, string time)
		{
			_store.Add(new PersonStateChangedEvent
			{
				PersonId = personId,
				BelongsToDate = time.Date(),
				Timestamp = time.Utc(),
			});
			return this;
		}

		public FakeRtaHistory BackFromLateForWork(Guid personId, string shiftStart, string time) =>
			BackFromLateForWork(personId, shiftStart, time, null, null, null, null, null, null);

		public FakeRtaHistory BackFromLateForWork(Guid personId, string shiftStart, string time, string state, string activity, Color? activityColor, string rule, Color? ruleColor, Adherence? adherence)
		{
			_store.Add(new PersonInAdherenceAfterLateForWorkEvent
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