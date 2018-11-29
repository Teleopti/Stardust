using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;


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
			 var virtualSchedulePeriodForWeekStart = _virtualSchedulePeriodProvider.VirtualSchedulePeriodForDate(date);
			 var virtualSchedulePeriodForWeekEnd = _virtualSchedulePeriodProvider.VirtualSchedulePeriodForDate(date.AddDays(6));
			 var isWeekStartPeriodNull = virtualSchedulePeriodForWeekStart == null;
			 var isWeekEndPeriodNull = virtualSchedulePeriodForWeekEnd == null;

			 if ((!isWeekStartPeriodNull && virtualSchedulePeriodForWeekStart.IsValid) && (isWeekEndPeriodNull || !virtualSchedulePeriodForWeekEnd.IsValid))
			 {
				 var contract = virtualSchedulePeriodForWeekStart.Contract;
				 weeklyWorkTimeSetting.MinWorkTimePerWeekMinutes = contract.WorkTimeDirective.MinTimePerWeek.TotalMinutes;
				 weeklyWorkTimeSetting.MaxWorkTimePerWeekMinutes = contract.WorkTimeDirective.MaxTimePerWeek.TotalMinutes;
			 }
			 else if ((!isWeekEndPeriodNull && virtualSchedulePeriodForWeekEnd.IsValid) && (isWeekStartPeriodNull || !virtualSchedulePeriodForWeekStart.IsValid))
			 {
				 var contract = virtualSchedulePeriodForWeekEnd.Contract;
				 weeklyWorkTimeSetting.MinWorkTimePerWeekMinutes = contract.WorkTimeDirective.MinTimePerWeek.TotalMinutes;
				 weeklyWorkTimeSetting.MaxWorkTimePerWeekMinutes = contract.WorkTimeDirective.MaxTimePerWeek.TotalMinutes;
			 }
			 else if ((!isWeekStartPeriodNull && virtualSchedulePeriodForWeekStart.IsValid) && (!isWeekEndPeriodNull && virtualSchedulePeriodForWeekEnd.IsValid))
			 {
				 var contractStart = virtualSchedulePeriodForWeekStart.Contract;
				 var contractEnd = virtualSchedulePeriodForWeekEnd.Contract;
				 weeklyWorkTimeSetting.MinWorkTimePerWeekMinutes = contractStart.WorkTimeDirective.MinTimePerWeek.TotalMinutes >
				                                                   contractEnd.WorkTimeDirective.MinTimePerWeek.TotalMinutes
					 ? contractEnd.WorkTimeDirective.MinTimePerWeek.TotalMinutes
					 : contractStart.WorkTimeDirective.MinTimePerWeek.TotalMinutes;
				 weeklyWorkTimeSetting.MaxWorkTimePerWeekMinutes = contractStart.WorkTimeDirective.MaxTimePerWeek.TotalMinutes >
				                                                   contractEnd.WorkTimeDirective.MaxTimePerWeek.TotalMinutes
					 ? contractStart.WorkTimeDirective.MaxTimePerWeek.TotalMinutes
					 : contractEnd.WorkTimeDirective.MaxTimePerWeek.TotalMinutes;
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
