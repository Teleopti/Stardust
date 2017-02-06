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

		public StaffingPossibilityViewModel CreateAbsencePossibilityViewModel()
		{
			return new StaffingPossibilityViewModel
			{
				SiteOpenHourPeriod = getIntradaySiteOpenHourPeriod(),
				Possibilities = new List<PeriodStaffingPossibilityViewModel>
				{
					new PeriodStaffingPossibilityViewModel
					{
						StartTime = DateTime.Now.AddMinutes(30),
						EndTime = DateTime.Now.AddMinutes(60),
						Possibility = 1
					},
					new PeriodStaffingPossibilityViewModel
					{
						StartTime = DateTime.Now.AddMinutes(60),
						EndTime = DateTime.Now.AddMinutes(90),
						Possibility = 2
					}
				}
			};
		}

		public StaffingPossibilityViewModel CreateOvertimePossibilityViewModel()
		{
			return new StaffingPossibilityViewModel
			{
				SiteOpenHourPeriod = getIntradaySiteOpenHourPeriod(),
				Possibilities = new List<PeriodStaffingPossibilityViewModel>
				{
					new PeriodStaffingPossibilityViewModel
					{
						StartTime = DateTime.Now.AddMinutes(30),
						EndTime = DateTime.Now.AddMinutes(60),
						Possibility = 1
					},
					new PeriodStaffingPossibilityViewModel
					{
						StartTime = DateTime.Now.AddMinutes(60),
						EndTime = DateTime.Now.AddMinutes(90),
						Possibility = 2
					}
				}
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