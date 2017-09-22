using System;
using System.Collections;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Areas.Reporting.Core;
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

			if ((isDayOff(schedule1) || isFullDayAbsence(schedule1)) && !isDayOff(schedule2) && !isFullDayAbsence(schedule2))
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

			if (!isDayOff(schedule1) && !isFullDayAbsence(schedule1) && (isDayOff(schedule2) || isFullDayAbsence(schedule2)))
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
			var fixedReferencePoint = schedule.ScheduleDay.DateOnlyAsPeriod.DateOnly.ToDateTimePeriod(TimeZoneInfo.Utc).StartDateTime;
			return (int)schedule.PersonAssignment.Value.Period.StartDateTime.Subtract(fixedReferencePoint).TotalMinutes;
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

	public class TeamScheduleTimeComparer : IComparer
	{
		private readonly Func<DateTimePeriod, DateTime> _predicate;
		private readonly IPermissionProvider _permissionProvider;
		private ScheduleVisibleReasons _scheduleVisibleReason = ScheduleVisibleReasons.Published;
		private StringComparer _stringComparer;

		public TeamScheduleTimeComparer(Func<DateTimePeriod, DateTime> predicate, IPermissionProvider permissionProvider, StringComparer stringComparer)
		{
			_predicate = predicate;
			_permissionProvider = permissionProvider;
			_stringComparer = stringComparer;
		}

		public int Compare(object x, object y)
		{
			var personSchedule1 = (Tuple<IPerson, IScheduleDay>)x;
			var personSchedule2 = (Tuple<IPerson, IScheduleDay>)y;

			var isPS1Empty = isEmptySchedule(personSchedule1.Item2);
			var isPS2Empty = isEmptySchedule(personSchedule2.Item2);
			var isPS1DayOff = isDayOff(personSchedule1.Item2);
			var isPS2DayOff = isDayOff(personSchedule2.Item2);

			if (isPS1Empty && isPS2Empty || (isPS1DayOff && isPS2DayOff))
			{
				return _stringComparer.Compare(personSchedule1.Item1.Name.LastName, personSchedule2.Item1.Name.LastName);
			}

			if (isPS1Empty || isPS1DayOff && !isPS2Empty) return 1;
			if (isPS2Empty || isPS2DayOff) return -1;

			var ps1 = personSchedule1.Item2.PersonAssignment();
			var time1 = ps1 != null ? _predicate(ps1.Period) : _predicate(personSchedule1.Item2.Period);

			var ps2 = personSchedule2.Item2?.PersonAssignment();
			var time2 = ps2 != null ? _predicate(ps2.Period) : _predicate(personSchedule2.Item2.Period);

			if (time1.Equals(time2))
			{
				return _stringComparer.Compare(personSchedule1.Item1.Name.LastName, personSchedule2.Item1.Name.LastName);
			}
			return time1.IsEarlierThan(time2) ? -1 : 1;

		}

		private bool isEmptySchedule(IScheduleDay schedule)
		{
			if (schedule == null)
			{
				return true;
			}
			var date = schedule.DateOnlyAsPeriod.DateOnly;
			var person = schedule.Person;
			var hasLayers = schedule.ProjectionService().CreateProjection().HasLayers;
			return !_permissionProvider.IsPersonSchedulePublished(date, person, _scheduleVisibleReason)
					&& !_permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.ViewUnpublishedSchedules) || !isDayOff(schedule) && !hasLayers;
		}

		private bool isDayOff(IScheduleDay schedule)
		{
			if (schedule == null) return false;
			var significantPart = schedule.SignificantPartForDisplay();
			return schedule.HasDayOff() || significantPart == SchedulePartView.ContractDayOff;
		}
	}
}