using System;

namespace Teleopti.Interfaces.Messages.Rta
{
	public class AgentAdherenceStateInfo
	{
		public Guid PersonId { get; set; }
		public string State { get; set; }
		public string Activity { get; set; }
		public string NextActivity { get; set; }
		public DateTime? NextActivityStartTime { get; set; }
		public string Alarm { get; set; }
		public DateTime? RuleStartTime { get; set; }
		public DateTime? StateStartTime { get; set; }
		public string AlarmColor { get; set; }

		public override bool Equals(object obj)
		{
			if (obj == null || GetType() != obj.GetType())
			{
				return false;
			}
			var state = (AgentAdherenceStateInfo) obj;
			return PersonId.Equals(state.PersonId) && State.Equals(state.State) && Activity.Equals(state.Activity) &&
			       NextActivity.Equals(state.NextActivity) && NextActivityStartTime.Equals(state.NextActivityStartTime)
				   && Alarm.Equals(state.Alarm) && RuleStartTime.Equals(state.RuleStartTime) && AlarmColor.Equals(state.AlarmColor) && StateStartTime.Equals(state.StateStartTime);
		}

		public override int GetHashCode()
		{
			return PersonId.GetHashCode() ^ State.GetHashCode() ^ Activity.GetHashCode() ^ NextActivity.GetHashCode() ^
			       NextActivityStartTime.GetHashCode() ^ Alarm.GetHashCode() ^ AlarmColor.GetHashCode() ^ RuleStartTime.GetHashCode() ^ StateStartTime.GetHashCode();
		}
	}
}