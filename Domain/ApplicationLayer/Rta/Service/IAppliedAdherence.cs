
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public interface IAppliedAdherence
	{
		Adherence ForAlarm(IAlarmType alarmType);
		EventAdherence StateToEvent(AdherenceState adherenceState);
	}

	public class ByStaffingEffect : IAppliedAdherence
	{
		public Adherence ForAlarm(IAlarmType alarmType)
		{
			if (alarmType == null)
				return Adherence.In;
			return ForStaffingEffect(alarmType.StaffingEffect);
		}

		public EventAdherence StateToEvent(AdherenceState adherenceState)
		{
			return adherenceState == AdherenceState.Out 
				? EventAdherence.Out 
				: EventAdherence.In;
		}

		public static Adherence ForStaffingEffect(double staffingEffect)
		{
			return staffingEffect.Equals(0)
				? Adherence.In
				: Adherence.Out;
		}
	}

	public class BySetting : IAppliedAdherence
	{
		public Adherence ForAlarm(IAlarmType alarmType)
		{
			if (alarmType == null)
				return Adherence.Neutral;
			return alarmType.Adherence.HasValue
				? alarmType.Adherence.Value
				: ByStaffingEffect.ForStaffingEffect(alarmType.StaffingEffect);
		}

		public EventAdherence StateToEvent(AdherenceState adherenceState)
		{
			if (adherenceState == AdherenceState.In)
				return EventAdherence.In;
			if (adherenceState == AdherenceState.Out)
				return EventAdherence.Out;
			return EventAdherence.Neutral;
		}
	}
}