using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
	public class IsDayScheduled : IIsDayScheduled
	{
		public bool Check(IScheduleDay scheduleDay)
		{
			var partView = scheduleDay.SignificantPart();
			return (partView == SchedulePartView.FullDayAbsence || partView == SchedulePartView.DayOff ||
							partView == SchedulePartView.ContractDayOff || partView == SchedulePartView.MainShift);
		}
	}
}