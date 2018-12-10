using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Schedule.DaySchedule;


namespace Teleopti.Ccc.Web.Areas.MyTime.Core.DaySchedule.ViewModelFactory
{
	public interface IScheduleDayViewModelFactory
	{
		DayScheduleViewModel CreateDayViewModel(DateOnly dateOnly, StaffingPossiblityType staffingPossiblityType);
	}
}