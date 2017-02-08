using System;
using System.Collections.Generic;
using Teleopti.Ccc.Web.Areas.MyTime.Models.ScheduleStaffingPossibility;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.ViewModelFactory
{
	public class StaffingPossibilityViewModelFactory : IStaffingPossibilityViewModelFactory
	{
		public List<PeriodStaffingPossibilityViewModel> CreateIntradayPeriodStaffingPossibilityViewModels(
			StaffingPossiblity staffingPossiblity)
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
					Possibility = (int) Math.Round(random.NextDouble())
				});
				periodStart = periodStart.AddMinutes(intervalLengthInMinutes);
			}
			//*/

			return possibilities;
		}
	}
}