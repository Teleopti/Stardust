using System;
using System.Drawing;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class ProperAlarm
	{
		public DateTime? StartTime(StateRuleInfo state, AgentState stored, DateTime currentTime)
		{
			if (!state.IsAlarm())
				return null;

			if (state.RuleChanged())
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

		public string ColorTransition(AgentStateReadModel x, int? timeInAlarm)
		{
			return timeInAlarm.HasValue
				? ColorTranslator.ToHtml(Color.FromArgb(x.AlarmColor ?? Color.White.ToArgb()))
				: ColorTranslator.ToHtml(Color.FromArgb(x.RuleColor ?? Color.White.ToArgb()));
		}
	}
}