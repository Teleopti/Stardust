using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.TeamSchedule.Core
{
	public static class TeamScheduleSortingUtil
	{
		public static int GetSortedValue(IScheduleDay schedule, bool hasPermissionForUnpublishedSchedule, bool isSchedulePublished, bool isFullAbsenceBeforeDayOff = true)
		{
			var fullAbsenceBase = isFullAbsenceBeforeDayOff ? 5000 : 10000;
			var dayOffBase = isFullAbsenceBeforeDayOff ? 10000 : 5000;
			if (schedule == null || !schedule.IsScheduled())
			{
				return 20000;
			}
			var significantPart = schedule.SignificantPart();
			if ((!isSchedulePublished && !hasPermissionForUnpublishedSchedule) || (schedule.PersonAssignment() == null && significantPart != SchedulePartView.FullDayAbsence))
			{
				return 20000;
			}

			if (schedule.HasDayOff() || significantPart == SchedulePartView.ContractDayOff)
			{
				return dayOffBase;
			}

			if (!schedule.HasDayOff() && significantPart == SchedulePartView.FullDayAbsence)
			{
				var mininumAbsenceStartTime = schedule.PersonAssignment() != null
					? schedule.PersonAssignment().Period.StartDateTime
					: schedule.PersonAbsenceCollection().Select(personAbsence => personAbsence.Period.StartDateTime).Min();

				return fullAbsenceBase + (int)mininumAbsenceStartTime.Subtract(schedule.DateOnlyAsPeriod.DateOnly.Date).TotalMinutes;
			}
			if (schedule.PersonAssignment() != null)
			{
				return (int)schedule.PersonAssignment().Period.StartDateTime.Subtract(schedule.DateOnlyAsPeriod.DateOnly.Date).TotalMinutes;
			}

			return 0;
		}
	}
}