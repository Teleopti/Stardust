using System;
using System.Globalization;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Tracking;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Interfaces.Domain;

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

			var tracker = absence.Tracker;
			var trackerType = getTrackerType(tracker);
			return new AbsenceAccountViewModel
			{
				AbsenceName = absence.Name,
				TrackerType = trackerType,
				PeriodStart = TimeZoneInfo.ConvertTimeFromUtc(absenceAccount.StartDate.Date, _userTimeZone.TimeZone()),
				PeriodEnd = TimeZoneInfo.ConvertTimeFromUtc(absenceAccount.Period().EndDate.Date, _userTimeZone.TimeZone()),
				Accrued = tracker == null ? string.Empty : convertTimeSpanToString(absenceAccount.Accrued, trackerType),
				Used = tracker == null ? string.Empty : convertTimeSpanToString(absenceAccount.LatestCalculatedBalance, trackerType),
				Remaining = tracker == null ? string.Empty : convertTimeSpanToString(absenceAccount.Remaining, trackerType)
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