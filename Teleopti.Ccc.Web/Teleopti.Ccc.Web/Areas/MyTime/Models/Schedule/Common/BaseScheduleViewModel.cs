using System.Collections.Generic;

namespace Teleopti.Ccc.Web.Areas.MyTime.Models.Schedule.Common
{
	public class BaseScheduleViewModel
	{
		public RequestPermission RequestPermission { get; set; }
		public IEnumerable<TimeLineViewModel> TimeLine { get; set; }
		public bool AsmEnabled { get; set; }
		public bool ViewPossibilityPermission { get; set; }
		public string DatePickerFormat { get; set; }
		public DaylightSavingsTimeAdjustmentViewModel DaylightSavingTimeAdjustment { get; set; }
		public double BaseUtcOffsetInMinutes { get; set; }
		public bool CheckStaffingByIntraday { get; set; }
		public bool AbsenceProbabilityEnabled { get; set; }
		public bool OvertimeProbabilityEnabled { get; set; }
		public int StaffingInfoAvailableDays { get; set; }
	}
}