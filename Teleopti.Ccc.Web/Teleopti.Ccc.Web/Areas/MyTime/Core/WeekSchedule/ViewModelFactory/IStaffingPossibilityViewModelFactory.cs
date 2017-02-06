using Teleopti.Ccc.Web.Areas.MyTime.Models.ScheduleStaffingPossibility;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.ViewModelFactory
{
	public interface IStaffingPossibilityViewModelFactory
	{
		StaffingPossibilityViewModel CreateAbsencePossibilityViewModel();

		StaffingPossibilityViewModel CreateOvertimePossibilityViewModel();
	}
}