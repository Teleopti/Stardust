using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public interface IAppliedAlarm
	{
		DateTime? StartTime(StateRuleInfo state, StoredStateInfo stored, DateTime currentTime);
		bool IsAlarm(StateRuleInfo state);
		bool RuleDefaultAlarm();
	}

	public class AllRulesIsAlarm : IAppliedAlarm
	{
		public DateTime? StartTime(StateRuleInfo state, StoredStateInfo stored, DateTime currentTime)
		{
			if (state.RuleId() == Guid.Empty)
				return null;

			if (state.HasRuleChanged())
				return currentTime.AddTicks(state.AlarmThresholdTime());
			return stored.AlarmStartTime;
		}

		public bool IsAlarm(StateRuleInfo state)
		{
			return true;
		}

		public bool RuleDefaultAlarm()
		{
			return true;
		}
	}

	public class ProperAlarm : IAppliedAlarm
	{
		public DateTime? StartTime(StateRuleInfo state, StoredStateInfo stored, DateTime currentTime)
		{
			if (!state.IsAlarm())
				return null;

			if (state.HasRuleChanged())
				return currentTime.AddTicks(state.AlarmThresholdTime());
			return stored.AlarmStartTime;
		}

		public bool IsAlarm(StateRuleInfo state)
		{
			return state.IsAlarm();
		}

		public bool RuleDefaultAlarm()
		{
			return false;
		}
	}
}