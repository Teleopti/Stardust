using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.DataProvider
{
	 public class PreferenceWeeklyWorkTimeSettingProvider : IPreferenceWeeklyWorkTimeSettingProvider
	{
		 private readonly IVirtualSchedulePeriodProvider _virtualSchedulePeriodProvider;

		 public PreferenceWeeklyWorkTimeSettingProvider(IVirtualSchedulePeriodProvider virtualSchedulePeriodProvider)
		 {
			 _virtualSchedulePeriodProvider = virtualSchedulePeriodProvider;
		 }

		 public WeeklyWorkTimeSetting RetrieveSetting(DateOnly date)
		 {
              var weeklyWorkTimeSetting = new WeeklyWorkTimeSetting();
			  var virtualSchedulePeriod = _virtualSchedulePeriodProvider.VirtualSchedulePeriodForDate(date);

		     if (virtualSchedulePeriod.IsValid)
		     {
		         var contract = virtualSchedulePeriod.Contract;
				 weeklyWorkTimeSetting.MinWorkTimePerWeekMinutes = contract.WorkTimeDirective.MinTimePerWeek.TotalMinutes;
				 weeklyWorkTimeSetting.MaxWorkTimePerWeekMinutes = contract.WorkTimeDirective.MaxTimePerWeek.TotalMinutes;
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
