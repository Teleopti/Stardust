using System;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.DataProvider
{
	 public class PreferenceWeeklyWorkTimeSettingProvider : IPreferenceWeeklyWorkTimeSettingProvider
	{
		 private readonly ILoggedOnUser _loggedOnUser;
		 private readonly IVirtualSchedulePeriodProvider _virtualSchedulePeriodProvider;

		 public PreferenceWeeklyWorkTimeSettingProvider(ILoggedOnUser loggedOnUser, IVirtualSchedulePeriodProvider virtualSchedulePeriodProvider)
		 {
			 _loggedOnUser = loggedOnUser;
			 _virtualSchedulePeriodProvider = virtualSchedulePeriodProvider;
		 }

		 public WeeklyWorkTimeSetting RetrieveSetting(DateOnly date)
		 {
              var weeklyWorkTimeSetting = new WeeklyWorkTimeSetting();
			  var virtualSchedulePeriod = _virtualSchedulePeriodProvider.VirtualSchedulePeriodForDate(date);

		     if (virtualSchedulePeriod.IsValid)
		     {
		         var contract = virtualSchedulePeriod.Contract;
		         var minTimePerWeekMinutes = _loggedOnUser.CurrentUser().WorkflowControlSet != null
		             ? _loggedOnUser.CurrentUser().WorkflowControlSet.MinTimePerWeek.TotalMinutes
		             : 0;

		         var maxTimePerWeekMinutes = contract.WorkTimeDirective.MaxTimePerWeek.TotalMinutes;

		         weeklyWorkTimeSetting.MinWorkTimePerWeekMinutes = minTimePerWeekMinutes;
		         weeklyWorkTimeSetting.MaxWorkTimePerWeekMinutes = maxTimePerWeekMinutes;
		     }

		     return weeklyWorkTimeSetting;
		 }
	}

	public class WeeklyWorkTimeSetting
	{
		public double MaxWorkTimePerWeekMinutes { get; set; }
		public double MinWorkTimePerWeekMinutes { get; set; }
	}
}
