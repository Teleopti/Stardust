using System;
using System.Drawing;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Wfm.Adherence;
using Teleopti.Wfm.Adherence.Configuration;
using Teleopti.Wfm.Adherence.Historical;
using Teleopti.Wfm.Adherence.Historical.Events;
using Teleopti.Wfm.Adherence.Historical.Infrastructure;
using Teleopti.Wfm.Adherence.States;
using Teleopti.Wfm.Adherence.States.Events;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeRtaHistory
	{
		private readonly IRtaEventStore _store;
		private readonly BelongsToDateMapper _belongsToDate;

		public FakeRtaHistory(IRtaEventStore store, BelongsToDateMapper belongsToDate)
		{
			_store = store;
			_belongsToDate = belongsToDate;
		}

		public FakeRtaHistory ShiftStart(Guid personId, string shiftStartTime, string shiftEndTime) =>
			ShiftStart(personId, null, shiftStartTime, shiftEndTime);

		public FakeRtaHistory ShiftStart(Guid personId, string date, string shiftStartTime, string shiftEndTime)
		{
			_store.Add(new PersonShiftStartEvent
			{
				PersonId = personId,
				BelongsToDate = belongsToDate(personId, shiftStartTime, date),
				ShiftStartTime = shiftStartTime.Utc(),
				ShiftEndTime = shiftEndTime.Utc()
			}, DeadLockVictim.No, RtaEventStoreVersion.StoreVersion);
			return this;
		}

		public FakeRtaHistory ShiftEnd(Guid personId, string shiftStartTime, string shiftEndTime) =>
			ShiftEnd(personId, null, shiftStartTime, shiftEndTime);

		public FakeRtaHistory ShiftEnd(Guid personId, string date, string shiftStartTime, string shiftEndTime)
		{
			_store.Add(new PersonShiftEndEvent
			{
				PersonId = personId,
				BelongsToDate = belongsToDate(personId, shiftStartTime, date),
				ShiftStartTime = shiftStartTime.Utc(),
				ShiftEndTime = shiftEndTime.Utc()
			}, DeadLockVictim.No, RtaEventStoreVersion.StoreVersion);
			return this;
		}

		public FakeRtaHistory StateChanged(Guid personId, string time) =>
			StateChanged(personId, time, null, null, null, null, null, null, null);

		public FakeRtaHistory StateChanged(Guid personId, string time, string date, string state, string activity, Color? activityColor, string rule, Color? ruleColor, Adherence? adherence)
		{
			_store.Add(new PersonStateChangedEvent
			{
				PersonId = personId,
				BelongsToDate = belongsToDate(personId, time, date),
				Timestamp = time.Utc(),
				StateName = state,
				ActivityName = activity,
				ActivityColor = activityColor?.ToArgb(),
				RuleName = rule,
				RuleColor = ruleColor?.ToArgb(),
				Adherence = adherence == null ? null : (EventAdherence?) Enum.Parse(typeof(EventAdherence), adherence.ToString()),
			}, DeadLockVictim.No, RtaEventStoreVersion.StoreVersion);
			return this;
		}

		public FakeRtaHistory RuleChanged(Guid personId, string time) =>
			RuleChanged(personId, time, null, null, null, null, null, null, null);

		public FakeRtaHistory RuleChanged(Guid personId, string time, string rule) =>
			RuleChanged(personId, time, null, null, null, null, rule, null, null);

		public FakeRtaHistory RuleChanged(Guid personId, string time, Adherence? adherence) =>
			RuleChanged(personId, time, null, null, null, null, null, null, adherence);

		public FakeRtaHistory RuleChanged(Guid personId, string time, string state, string activity, Color? activityColor, string rule, Color? ruleColor, Adherence? adherence) =>
			RuleChanged(personId, time, null, state, activity, activityColor, rule, ruleColor, adherence);

		public FakeRtaHistory RuleChanged(Guid personId, string time, string date, string state, string activity, Color? activityColor, string rule, Color? ruleColor, Adherence? adherence)
		{
			_store.Add(new PersonRuleChangedEvent
			{
				PersonId = personId,
				BelongsToDate = belongsToDate(personId, time, date),
				Timestamp = time.Utc(),
				StateName = state,
				ActivityName = activity,
				ActivityColor = activityColor?.ToArgb(),
				RuleName = rule,
				RuleColor = ruleColor?.ToArgb(),
				Adherence = adherence == null ? null : (EventAdherence?) Enum.Parse(typeof(EventAdherence), adherence.ToString()),
			}, DeadLockVictim.No, RtaEventStoreVersion.StoreVersion);
			return this;
		}

		public FakeRtaHistory ArrivedLateForWork(Guid personId, string shiftStart, string time) =>
			ArrivedLateForWork(personId, shiftStart, time, null, null, null, null, null, null, null);

		public FakeRtaHistory ArrivedLateForWork(Guid personId, string shiftStart, string time, string state, string activity, Color? activityColor, string rule, Color? ruleColor, Adherence? adherence) =>
			ArrivedLateForWork(personId, shiftStart, time, null, state, activity, activityColor, rule, ruleColor, adherence);

		public FakeRtaHistory ArrivedLateForWork(Guid personId, string shiftStart, string time, string date, string state, string activity, Color? activityColor, string rule, Color? ruleColor, Adherence? adherence)
		{
			_store.Add(new PersonArrivedLateForWorkEvent
			{
				PersonId = personId,
				BelongsToDate = belongsToDate(personId, time, date),
				Timestamp = time.Utc(),
				ShiftStart = shiftStart.Utc(),
				StateName = state,
				ActivityName = activity,
				ActivityColor = activityColor?.ToArgb(),
				RuleName = rule,
				RuleColor = ruleColor?.ToArgb(),
				Adherence = adherence == null ? null : (EventAdherence?) Enum.Parse(typeof(EventAdherence), adherence.ToString())
			}, DeadLockVictim.No, RtaEventStoreVersion.StoreVersion);
			return this;
		}

		public FakeRtaHistory AdherenceDayStart(Guid personId, string time, string state, string activity, Color? activityColor, string rule, Color? ruleColor, Adherence? adherence) =>
			AdherenceDayStart(personId, time, null, state, activity, activityColor, rule, ruleColor, adherence);

		public FakeRtaHistory AdherenceDayStart(Guid personId, string time, string date, string state, string activity, Color? activityColor, string rule, Color? ruleColor, Adherence? adherence)
		{
			_store.Add(new PersonAdherenceDayStartEvent
			{
				PersonId = personId,
				BelongsToDate = belongsToDate(personId, time, date),
				Timestamp = time.Utc(),
				StateName = state,
				ActivityName = activity,
				ActivityColor = activityColor?.ToArgb(),
				RuleName = rule,
				RuleColor = ruleColor?.ToArgb(),
				Adherence = adherence == null ? null : (EventAdherence?) Enum.Parse(typeof(EventAdherence), adherence.ToString())
			}, DeadLockVictim.No, RtaEventStoreVersion.StoreVersion);
			return this;
		}
		
		public FakeRtaHistory AdjustedAdherenceToNeutral(string start, string end)
		{
			_store.Add(new AdjustAdherenceToNeutralEvent
			{
				StartTime = start.Utc(),
				EndTime = end.Utc()
			}, DeadLockVictim.No, RtaEventStoreVersion.StoreVersion);
			return this;
		}

		private DateOnly? belongsToDate(Guid personId, string time, string date)
		{
			if (date == null)
				return _belongsToDate.BelongsToDate(personId, time.Utc(), time.Utc());
			return new DateOnly(date.Utc());
		}
	}
}