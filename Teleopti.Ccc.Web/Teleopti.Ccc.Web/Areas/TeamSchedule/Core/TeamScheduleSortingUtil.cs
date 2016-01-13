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
			var significantPart = schedule.SignificantPartForDisplay();
			var isFullDayAbsence = significantPart == SchedulePartView.FullDayAbsence ||
								   ((significantPart == SchedulePartView.ContractDayOff || significantPart == SchedulePartView.DayOff) &&
									schedule.ProjectionService().CreateProjection().HasLayers);

			if ((!isSchedulePublished && !hasPermissionForUnpublishedSchedule) || (schedule.PersonAssignment() == null && !isFullDayAbsence))
			{
				return 20000;
			}

			if (schedule.HasDayOff() || significantPart == SchedulePartView.ContractDayOff)
			{
				return dayOffBase;
			}

			if (!schedule.HasDayOff() && isFullDayAbsence)
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