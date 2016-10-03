using System;
using System.Collections;
using System.Linq;
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
			var schedule1 = new scheduleDayDetail(personSchedule1.Item2);
			var schedule2 = new scheduleDayDetail(personSchedule2.Item2);

			if (isEmptySchedule(schedule1) && isEmptySchedule(schedule2))
			{
				return string.CompareOrdinal(person1.Name.LastName, person2.Name.LastName);
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
				return string.CompareOrdinal(person1.Name.LastName, person2.Name.LastName);
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
				return string.CompareOrdinal(person1.Name.LastName, person2.Name.LastName);
			}

			if (!isDayOff(schedule1)&& !isFullDayAbsence(schedule1) && (isDayOff(schedule2)||isFullDayAbsence(schedule2)))
			{
				return -1;
			}

			if (schedule1.PersonAssignment.Value != null && schedule2.PersonAssignment.Value != null)
			{
				var offset1 = getMinActivityStartTimeOffset(schedule1);
				var offset2 = getMinActivityStartTimeOffset(schedule2);
				if (offset1 > offset2)
					return 1;
				if (offset1 < offset2)
					return -1;
				return string.CompareOrdinal(person1.Name.LastName, person2.Name.LastName);
			}

			return string.CompareOrdinal(person1.Name.LastName, person2.Name.LastName);
		}

		private int getMinActivityStartTimeOffset(scheduleDayDetail schedule)
		{
			return (int)schedule.PersonAssignment.Value.Period.StartDateTime.Subtract(schedule.ScheduleDay.Period.StartDateTime).TotalMinutes;
		}

		private int getMinAbsenceStartTimeOffset(scheduleDayDetail schedule)
		{
			var personAssignment = schedule.PersonAssignment.Value;
			var mininumAbsenceStartTime = personAssignment?.Period.StartDateTime ?? schedule.ScheduleDay.PersonAbsenceCollection().Select(personAbsence => personAbsence.Period.StartDateTime).Min();
			mininumAbsenceStartTime = schedule.ScheduleDay.Period.Contains(mininumAbsenceStartTime)
				? mininumAbsenceStartTime
				: schedule.ScheduleDay.Period.StartDateTime;

			return (int)mininumAbsenceStartTime.Subtract(schedule.ScheduleDay.Period.StartDateTime).TotalMinutes;
		}

		private bool isEmptySchedule(scheduleDayDetail schedule)
		{
			if (schedule.ScheduleDay == null) return true;
			var person = schedule.ScheduleDay.Person;
			var date = schedule.ScheduleDay.DateOnlyAsPeriod.DateOnly;
			var hasLayers = schedule.Projection.Value.HasLayers;
			return (!_permissionProvider.IsPersonSchedulePublished(date, person, _scheduleVisibleReason) && !_hasPermissionForUnpublishedSchedule) ||
						(!isDayOff(schedule) && !hasLayers);
		}

		private bool isFullDayAbsence(scheduleDayDetail schedule)
		{
			if (schedule.ScheduleDay == null) return false;
			var significantPart = schedule.SignificantPartForDisplay.Value;
			return !schedule.ScheduleDay.HasDayOff() && (significantPart == SchedulePartView.FullDayAbsence ||
			   (significantPart == SchedulePartView.DayOff &&
							  schedule.Projection.Value.HasLayers));
		}

		private bool isDayOff(scheduleDayDetail schedule)
		{
			if (schedule.ScheduleDay == null) return false;
			var significantPart = schedule.SignificantPartForDisplay.Value;
			return schedule.ScheduleDay.HasDayOff() || significantPart == SchedulePartView.ContractDayOff;
		}

		private class scheduleDayDetail
		{
			public scheduleDayDetail(IScheduleDay scheduleDay)
			{
				if (scheduleDay != null)
				{
					Projection = new Lazy<IVisualLayerCollection>(() => scheduleDay.ProjectionService().CreateProjection());
					SignificantPartForDisplay = new Lazy<SchedulePartView>(scheduleDay.SignificantPartForDisplay);
					PersonAssignment = new Lazy<IPersonAssignment>(() => scheduleDay.PersonAssignment());
				}
				ScheduleDay = scheduleDay;
			}

			public IScheduleDay ScheduleDay { get; }

			public Lazy<IPersonAssignment> PersonAssignment { get; }

			public Lazy<SchedulePartView> SignificantPartForDisplay { get; }

			public Lazy<IVisualLayerCollection> Projection { get; }
		}
	}
}