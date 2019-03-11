using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.MyTime.Models.ScheduleStaffingPossibility;


namespace Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.ViewModelFactory
{
	public interface IStaffingPossibilityViewModelFactory
	{
		IEnumerable<PeriodStaffingPossibilityViewModel> CreatePeriodStaffingPossibilityViewModelsForWeek(DateOnly startDate, StaffingPossibilityType staffingPossibilityType);

		IEnumerable<PeriodStaffingPossibilityViewModel> CreatePeriodStaffingPossibilityViewModelsForMobileDay(DateOnly startDate, StaffingPossibilityType staffingPossibilityType);
	}
}