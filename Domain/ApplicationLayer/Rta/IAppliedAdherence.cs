
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta
{
	public interface IAppliedAdherence
	{
		Adherence ForAlarm(IAlarmType alarmType);
	}

	public class ByStaffingEffect : IAppliedAdherence
	{
		public Adherence ForAlarm(IAlarmType alarmType)
		{
			return ForStaffingEffect(alarmType.StaffingEffect);
		}

		public static Adherence ForStaffingEffect(double staffingEffect)
		{
			return staffingEffect.Equals(0) ? Adherence.In : Adherence.Out;
		}
	}

	public class ByPolicy : IAppliedAdherence
	{
		public Adherence ForAlarm(IAlarmType alarmType)
		{
			return alarmType.Adherence.HasValue
				? alarmType.Adherence.Value
				: alarmType.StaffingEffect.Equals(0) ? Adherence.In : Adherence.Out;
		}
	}
}