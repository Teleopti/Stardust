using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.MyTime.Models.MonthSchedule;
using Teleopti.Ccc.Web.Areas.MyTime.Models.WeekSchedule;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.ViewModelFactory
{
	public interface IScheduleViewModelFactory
	{
		DayScheduleViewModel CreateDayViewModel(DateOnly dateOnly, StaffingPossiblityType staffingPossiblityType);

		WeekScheduleViewModel CreateWeekViewModel(DateOnly dateOnly, StaffingPossiblityType staffingPossiblityType);

		MonthScheduleViewModel CreateMonthViewModel(DateOnly dateOnly);
	}
}