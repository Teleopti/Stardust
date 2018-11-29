using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Restrictions
{
	public interface IAbsencePreferenceFullDayLayerCreator
	{
		AbsenceLayer Create(IScheduleDay scheduleDay, IAbsence absence);
	}

	public class AbsencePreferenceFullDayLayerCreator : IAbsencePreferenceFullDayLayerCreator
	{
		public AbsenceLayer Create(IScheduleDay scheduleDay, IAbsence absence)
		{
			var scheduleDayPeriod = scheduleDay.Period;
			var layerPeriod = new DateTimePeriod(scheduleDayPeriod.StartDateTime.AddHours(8), scheduleDayPeriod.StartDateTime.AddHours(20));
			return new AbsenceLayer(absence, layerPeriod);
		}
	}
}
