using System.Collections.Generic;
using Teleopti.Ccc.Web.Areas.MyTime.Models.ScheduleStaffingPossibility;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.ViewModelFactory
{
	public interface IStaffingPossibilityViewModelFactory
	{
		List<PeriodStaffingPossibilityViewModel> CreateIntradayPeriodStaffingPossibilityViewModels(StaffingPossiblity staffingPossiblity);
	}
}