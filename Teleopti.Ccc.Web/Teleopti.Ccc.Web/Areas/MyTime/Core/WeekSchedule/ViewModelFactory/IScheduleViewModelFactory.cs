using Teleopti.Ccc.Web.Areas.MyTime.Models.MonthSchedule;
using Teleopti.Ccc.Web.Areas.MyTime.Models.WeekSchedule;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.ViewModelFactory
{
	public interface IScheduleViewModelFactory
	{
		WeekScheduleViewModel CreateWeekViewModel(DateOnly dateOnly);
        MonthScheduleViewModel CreateMonthViewModel(DateOnly dateOnly);
	}
}