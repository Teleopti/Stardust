using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public interface IAppliedAlarmStartTime
	{
		DateTime? For(StateAlarmInfo state, StoredStateInfo stored, DateTime currentTime);
	}

	public class OldAlarmStartTime : IAppliedAlarmStartTime
	{
		public DateTime? For(StateAlarmInfo state, StoredStateInfo stored, DateTime currentTime)
		{
			if (state.RuleId() == Guid.Empty)
				return null;
			if (stored.RuleId() == state.RuleId())
				return stored.AlarmStartTime;
			return currentTime.AddTicks(state.AlarmThresholdTime());
		}
	}

	public class IsAlarmStartTime : IAppliedAlarmStartTime
	{
		public DateTime? For(StateAlarmInfo state, StoredStateInfo stored, DateTime currentTime)
		{
			if (state.IsInAlarm())
			{
				if (state.HasRuleChanged())
					return currentTime.AddTicks(state.AlarmThresholdTime());
				return stored.AlarmStartTime;
			}

			return null;
		}
	}
}