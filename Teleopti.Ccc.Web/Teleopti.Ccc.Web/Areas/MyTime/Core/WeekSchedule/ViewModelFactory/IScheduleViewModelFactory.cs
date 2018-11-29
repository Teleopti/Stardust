using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.MyTime.Models.MonthSchedule;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Schedule.WeekSchedule;


namespace Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.ViewModelFactory
{
	public interface IScheduleViewModelFactory
	{
		WeekScheduleViewModel CreateWeekViewModel(DateOnly dateOnly, StaffingPossiblityType staffingPossiblityType);

		MonthScheduleViewModel CreateMonthViewModel(DateOnly dateOnly);

		MonthScheduleViewModel CreateMobileMonthViewModel(DateOnly dateOnly);
	}
}