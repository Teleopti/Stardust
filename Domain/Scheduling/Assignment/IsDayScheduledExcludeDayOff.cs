using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
	public class IsDayScheduledExcludeDayOff : IIsDayScheduled
	{
		public bool Check(IScheduleDay scheduleDay)
		{
			var partView = scheduleDay.SignificantPart();
			return (partView == SchedulePartView.FullDayAbsence || 
							partView == SchedulePartView.ContractDayOff || partView == SchedulePartView.MainShift);
		}
	}
}