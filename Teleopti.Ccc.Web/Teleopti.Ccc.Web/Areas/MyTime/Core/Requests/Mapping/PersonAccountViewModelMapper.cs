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
				TrackerType = trackerType,
				PeriodStart = TimeZoneInfo.ConvertTimeFromUtc(absenceAccount.StartDate.Date, timezone),
				PeriodEnd = TimeZoneInfo.ConvertTimeFromUtc(absenceAccount.Period().EndDate.Date, timezone),
				Accrued = convertTimeSpanToString(absenceAccount.Accrued, trackerType),
				Used = convertTimeSpanToString(absenceAccount.LatestCalculatedBalance, trackerType),
				Remaining = convertTimeSpanToString(absenceAccount.Remaining, trackerType)
			};
		}

		private static string getTrackerType(ITracker tracker)
		{
			var trackerType = trackerTypeOthers;
			if (tracker == null) return trackerType;

			var classTypeOfTracker = tracker.GetType();
			if (classTypeOfTracker == Tracker.CreateDayTracker().GetType())
			{
				trackerType = trackerTypeDays;
			}
			else if (classTypeOfTracker == Tracker.CreateTimeTracker().GetType())
			{
				trackerType = trackerTypeHours;
			}
			return trackerType;
		}

		private static string convertTimeSpanToString(TimeSpan ts, string trackerType)
		{
			switch (trackerType)
			{
				case trackerTypeDays:
					return ts.TotalDays.ToString(CultureInfo.CurrentCulture);

				case trackerTypeHours:
					return TimeHelper.GetLongHourMinuteTimeString(ts, CultureInfo.CurrentCulture);

				default:
					return string.Empty;
			}
		}
	}
}