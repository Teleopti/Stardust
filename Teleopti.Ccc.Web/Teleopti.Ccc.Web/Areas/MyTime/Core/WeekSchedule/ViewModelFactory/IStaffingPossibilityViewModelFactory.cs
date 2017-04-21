using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.MyTime.Models.ScheduleStaffingPossibility;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.ViewModelFactory
{
	public interface IStaffingPossibilityViewModelFactory
	{
		IEnumerable<PeriodStaffingPossibilityViewModel> CreatePeriodStaffingPossibilityViewModels(DateOnly startDate, StaffingPossiblityType staffingPossiblityType);
	}
}