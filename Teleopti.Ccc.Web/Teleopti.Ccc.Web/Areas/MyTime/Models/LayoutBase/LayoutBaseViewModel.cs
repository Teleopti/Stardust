using System;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Shared;
using Teleopti.Ccc.Web.Areas.MyTime.Models.WeekSchedule;

namespace Teleopti.Ccc.Web.Areas.MyTime.Models.LayoutBase
{
	public class LayoutBaseViewModel
	{
		public string Title { get; set; }
		public CultureSpecificViewModel CultureSpecific { get; set; }
		public string Footer { get; set; }
		public DatePickerGlobalizationViewModel DatePickerGlobalization { get; set; }
		public DateTime? FixedDate { get; set; }
		public string Version { get; set; }
		public int UserTimezoneOffsetMinute { get; set; }
		public bool HasDayLightSaving { get; set; }
		public string DayLightSavingStart { get; set; }
		public string DayLightSavingEnd { get; set; }
		public int DayLightSavingAdjustmentInMinute { get; set; }
	}
}