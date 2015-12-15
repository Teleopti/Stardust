namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public interface IAppliedIsAlarm
	{
		bool IsAlarm(StateAlarmInfo state);
	}

	public class EveryRuleIsAlarm : IAppliedIsAlarm
	{
		public bool IsAlarm(StateAlarmInfo state)
		{
			return true;
		}
	}

	public class RuleSpecifiedIsAlarm : IAppliedIsAlarm
	{
		public bool IsAlarm(StateAlarmInfo state)
		{
			return state.IsInAlarm();
		}
	}
}
