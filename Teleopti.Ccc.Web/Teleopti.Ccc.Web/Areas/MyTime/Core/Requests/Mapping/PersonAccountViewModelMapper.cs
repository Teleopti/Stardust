using System;
using System.Globalization;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Tracking;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping
{
	public class PersonAccountViewModelMapper
	{
		private const string trackerTypeOthers = "Others";
		private const string trackerTypeDays = "Days";
		private const string trackerTypeHours = "Hours";

		private readonly IUserTimeZone _userTimeZone;

		private static readonly Type DayTracker = Tracker.CreateDayTracker().GetType();
		private static readonly Type TimeTracker = Tracker.CreateTimeTracker().GetType();

		public PersonAccountViewModelMapper(IUserTimeZone timeZone)
		{
			_userTimeZone = timeZone;
		}

		public AbsenceAccountViewModel Map(IAccount absenceAccount)
		{
			var absence = absenceAccount?.Owner?.Absence;
			if (absence == null) return null;

			var timezone = _userTimeZone.TimeZone();
			var trackerType = getTrackerType(absence.Tracker);
			return new AbsenceAccountViewModel
			{
				AbsenceName = absence.Name,
				TrackerType = trackerType.Item1,
				PeriodStart = TimeZoneInfo.ConvertTimeFromUtc(absenceAccount.StartDate.Date, timezone),
				PeriodEnd = TimeZoneInfo.ConvertTimeFromUtc(absenceAccount.Period().EndDate.Date, timezone),
				Accrued = trackerType.Item2(absenceAccount.Accrued),
				Used = trackerType.Item2(absenceAccount.LatestCalculatedBalance),
				Remaining = trackerType.Item2(absenceAccount.Remaining)
			};
		}

		private static (string,Func<TimeSpan,string>) getTrackerType(ITracker tracker)
		{
			if (tracker == null) return (trackerTypeOthers, _ => string.Empty);

			var classTypeOfTracker = tracker.GetType();
			if (classTypeOfTracker == DayTracker)
			{
				return (trackerTypeDays, ts => ts.TotalDays.ToString(CultureInfo.CurrentCulture));
			}

			if (classTypeOfTracker == TimeTracker)
			{
				return (trackerTypeHours, ts => TimeHelper.GetLongHourMinuteTimeString(ts, CultureInfo.CurrentCulture));
			}

			return (trackerTypeOthers, _ => string.Empty);
		}
	}
}