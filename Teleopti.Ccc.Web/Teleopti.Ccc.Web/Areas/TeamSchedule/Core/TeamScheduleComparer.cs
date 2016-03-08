using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.TeamSchedule.Core
{
	public class TeamScheduleComparer : IComparer
	{
		private readonly bool _hasPermissionForUnpublishedSchedule;
		private readonly IPermissionProvider _permissionProvider;
		private readonly bool _isFullAbsenceBeforeDayOff;
		private ScheduleVisibleReasons _scheduleVisibleReason;

		public TeamScheduleComparer(bool hasPermissionForUnpublishedSchedule, IPermissionProvider permissionProvider, bool isFullAbsenceBeforeDayOff = true)
		{
			_hasPermissionForUnpublishedSchedule = hasPermissionForUnpublishedSchedule;
			_permissionProvider = permissionProvider;
			_isFullAbsenceBeforeDayOff = isFullAbsenceBeforeDayOff;
			_scheduleVisibleReason = ScheduleVisibleReasons.Published;
		}

		public ScheduleVisibleReasons ScheduleVisibleReason
		{
			set { _scheduleVisibleReason = value; } 
		}

		public int Compare(object x, object y)
		{
			var personSchedule1 = (Tuple<IPerson, IScheduleDay>)x;
			var personSchedule2 = (Tuple<IPerson, IScheduleDay>)y;
			var person1 = personSchedule1.Item1;
			var person2 = personSchedule2.Item1;
			var schedule1 = personSchedule1.Item2;
			var schedule2 = personSchedule2.Item2;

			if (isEmptySchedule(schedule1) && isEmptySchedule(schedule2))
			{
				return String.Compare(person1.Name.LastName, person2.Name.LastName);
			}
			if (isEmptySchedule(schedule1) && !isEmptySchedule(schedule2))
			{
				return 1;
			}
			if (!isEmptySchedule(schedule1) && isEmptySchedule(schedule2))
			{
				return -1;
			}

			if (isDayOff(schedule1) && isFullDayAbsence(schedule2))
			{
				return _isFullAbsenceBeforeDayOff ? 1 : -1;
			}
			if ((isDayOff(schedule1)|| isFullDayAbsence(schedule1)) && !isDayOff(schedule2) && !isFullDayAbsence(schedule2))
			{
				return 1;
			}

			if (isDayOff(schedule1) && isDayOff(schedule2))
			{
				return String.Compare(person1.Name.LastName, person2.Name.LastName);
			}

			if (isFullDayAbsence(schedule1) && isDayOff(schedule2))
			{
				return _isFullAbsenceBeforeDayOff ? -1 : 1;
			}

			if (isFullDayAbsence(schedule1) && isFullDayAbsence(schedule2))
			{
				var offset1 = getMinAbsenceStartTimeOffset(schedule1);
				var offset2 = getMinAbsenceStartTimeOffset(schedule2);
				if (offset1 > offset2)
					return 1;
				if (offset1 < offset2)
					return -1;
				return String.Compare(person1.Name.LastName, person2.Name.LastName);
			}

			if (!isDayOff(schedule1)&& !isFullDayAbsence(schedule1) && (isDayOff(schedule2)||isFullDayAbsence(schedule2)))
			{
				return -1;
			}

			if (schedule1.PersonAssignment() != null && schedule2.PersonAssignment() != null)
			{
				var offset1 = getMinActivityStartTimeOffset(schedule1);
				var offset2 = getMinActivityStartTimeOffset(schedule2);
				if (offset1 > offset2)
					return 1;
				if (offset1 < offset2)
					return -1;
				return String.Compare(person1.Name.LastName, person2.Name.LastName);
			}

			return String.Compare(person1.Name.LastName, person2.Name.LastName);
		}

		private int getMinActivityStartTimeOffset(IScheduleDay schedule)
		{
			return (int)schedule.PersonAssignment().Period.StartDateTime.Subtract(schedule.DateOnlyAsPeriod.DateOnly.Date).TotalMinutes;
		}

		private int getMinAbsenceStartTimeOffset(IScheduleDay schedule)
		{
			var mininumAbsenceStartTime = schedule.PersonAssignment() != null
					? schedule.PersonAssignment().Period.StartDateTime
					: schedule.PersonAbsenceCollection().Select(personAbsence => personAbsence.Period.StartDateTime).Min();
			mininumAbsenceStartTime = mininumAbsenceStartTime.Date == schedule.DateOnlyAsPeriod.DateOnly.Date
				? mininumAbsenceStartTime
				: schedule.DateOnlyAsPeriod.DateOnly.Date;

			return (int)mininumAbsenceStartTime.Subtract(schedule.DateOnlyAsPeriod.DateOnly.Date).TotalMinutes;
		}

		private bool isEmptySchedule(IScheduleDay schedule)
		{
			if (schedule == null) return true;
			var person = schedule.Person;
			var date = schedule.DateOnlyAsPeriod.DateOnly;
			var hasLayers = schedule.ProjectionService().CreateProjection().HasLayers;
			return (!_permissionProvider.IsPersonSchedulePublished(date, person, _scheduleVisibleReason) && !_hasPermissionForUnpublishedSchedule) ||
						(!isDayOff(schedule) && !hasLayers);
		}

		private bool isFullDayAbsence(IScheduleDay schedule)
		{
			if (schedule == null) return false;
			var significantPart = schedule.SignificantPartForDisplay();
			return !schedule.HasDayOff() && (significantPart == SchedulePartView.FullDayAbsence ||
			   (significantPart == SchedulePartView.DayOff &&
							  schedule.ProjectionService().CreateProjection().HasLayers));
		}

		private bool isDayOff(IScheduleDay schedule)
		{
			if (schedule == null) return false;
			var significantPart = schedule.SignificantPartForDisplay();
			return schedule.HasDayOff() || significantPart == SchedulePartView.ContractDayOff;
		}
	}
}