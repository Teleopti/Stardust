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
		private readonly IUserTimeZone _userTimeZone;

		public PersonAccountViewModelMapper(IUserTimeZone timeZone)
		{
			_userTimeZone = timeZone;
		}

		public AbsenceAccountViewModel Map(IAccount m)
		{
			return new AbsenceAccountViewModel
			{
				AbsenceName = m.Owner.Absence.Name,
				TrackerType = trackerType(m),
				PeriodStart = TimeZoneInfo.ConvertTimeFromUtc(m.StartDate.Date, _userTimeZone.TimeZone()),
				PeriodEnd = TimeZoneInfo.ConvertTimeFromUtc(m.Period().EndDate.Date, _userTimeZone.TimeZone()),
				Accrued = convertTimeSpanToString(m.Accrued, m.Owner.Absence.Tracker),
				Used = convertTimeSpanToString(m.LatestCalculatedBalance, m.Owner.Absence.Tracker),
				Remaining = convertTimeSpanToString(m.Remaining, m.Owner.Absence.Tracker)
			};
		}

		private static string trackerType(IAccount s)
		{
			var trackerType = "Others";
			var classTypeOfTracker = s.Owner.Absence.Tracker.GetType();
			if (classTypeOfTracker == Tracker.CreateDayTracker().GetType())
			{
				trackerType = "Days";
			}
			else if (classTypeOfTracker == Tracker.CreateTimeTracker().GetType())
			{
				trackerType = "Hours";
			}
			return trackerType;
		}

		private static string convertTimeSpanToString(TimeSpan ts, ITracker tracker)
		{
			var result = string.Empty;

			var classTypeOfTracker = tracker.GetType();
			if (classTypeOfTracker == Tracker.CreateDayTracker().GetType())
			{
				result = ts.TotalDays.ToString(CultureInfo.CurrentCulture);
			}
			else if (classTypeOfTracker == Tracker.CreateTimeTracker().GetType())
			{
				result = TimeHelper.GetLongHourMinuteTimeString(ts, CultureInfo.CurrentCulture);
			}

			return result;
		}
	}
}