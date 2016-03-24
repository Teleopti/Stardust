using System.Drawing;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public interface IAppliedColor
	{
		string ColorTransition(AgentStateReadModel x, int? timeInAlarm);
	}

	public class RuleColor : IAppliedColor
	{
		public string ColorTransition(AgentStateReadModel x, int? timeInAlarm)
		{
			return ColorTranslator.ToHtml(Color.FromArgb(x.RuleColor ?? Color.White.ToArgb()));
		}
	}

	public class ProperAlarmColor : IAppliedColor
	{
		public string ColorTransition(AgentStateReadModel x, int? timeInAlarm)
		{
			return timeInAlarm.HasValue
				? ColorTranslator.ToHtml(Color.FromArgb(x.AlarmColor ?? Color.White.ToArgb()))
				: ColorTranslator.ToHtml(Color.FromArgb(x.RuleColor ?? Color.White.ToArgb()));
		}
	}
}