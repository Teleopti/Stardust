using System;
using System.Collections.Generic;
using Teleopti.Ccc.Web.Areas.MyTime.Models.ScheduleStaffingPossibility;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.ViewModelFactory
{
	public class StaffingPossibilityViewModelFactory : IStaffingPossibilityViewModelFactory
	{
		private readonly ILoggedOnUser _loggedOnUser;

		public StaffingPossibilityViewModelFactory(ILoggedOnUser loggedOnUser)
		{
			_loggedOnUser = loggedOnUser;
		}

		public StaffingPossibilityViewModel CreateIntradayAbsencePossibilityViewModel()
		{
			//* TODO: This is just for creating demo data, should be replaced with API invoking
			const int intervalLengthInMinutes = 15;
			var today = DateTime.Now.Date;
			var tomorrow = today.AddDays(1);

			var random = new Random();
			var periodStart = today;
			var possibilities = new List<PeriodStaffingPossibilityViewModel>();
			while (periodStart < tomorrow)
			{
				possibilities.Add(new PeriodStaffingPossibilityViewModel
				{
					StartTime = periodStart,
					EndTime = periodStart.AddMinutes(intervalLengthInMinutes),
					Possibility = (int)Math.Round(random.NextDouble())
				});
				periodStart = periodStart.AddMinutes(intervalLengthInMinutes);
			}
			//*/

			return new StaffingPossibilityViewModel
			{
				SiteOpenHourPeriod = getIntradaySiteOpenHourPeriod(),
				Possibilities = possibilities
			};
		}

		public StaffingPossibilityViewModel CreateIntradayOvertimePossibilityViewModel()
		{
			//* TODO: This is just for creating demo data, should be replaced with API invoking
			const int intervalLengthInMinutes = 15;
			var today = DateTime.Now.Date;
			var tomorrow = today.AddDays(1);

			var random = new Random();
			var periodStart = today;
			var possibilities = new List<PeriodStaffingPossibilityViewModel>();
			while (periodStart < tomorrow)
			{
				possibilities.Add(new PeriodStaffingPossibilityViewModel
				{
					StartTime = periodStart,
					EndTime = periodStart.AddMinutes(intervalLengthInMinutes),
					Possibility = (int)Math.Round(random.NextDouble())
				});
				periodStart = periodStart.AddMinutes(intervalLengthInMinutes);
			}
			//*/

			return new StaffingPossibilityViewModel
			{
				SiteOpenHourPeriod = getIntradaySiteOpenHourPeriod(),
				Possibilities = possibilities
			};
		}

		private TimePeriod getIntradaySiteOpenHourPeriod()
		{
			TimePeriod timePeriod;
			var siteOpenHour = _loggedOnUser.CurrentUser().SiteOpenHour(DateOnly.Today);
			if (siteOpenHour == null)
			{
				timePeriod = new TimePeriod(TimeSpan.Zero, TimeSpan.FromHours(24).Subtract(TimeSpan.FromSeconds(1)));
			}
			else
			{
				timePeriod = siteOpenHour.IsClosed ? new TimePeriod() : siteOpenHour.TimePeriod;
			}
			return timePeriod;
		}
	}
}