using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.ViewModelFactory
{
	public interface IStaffingDataAvailablePeriodProvider
	{
		DateOnlyPeriod? GetPeriodForAbsence(DateOnly date, bool forThisWeek);
		List<DateOnlyPeriod> GetPeriodsForOvertime(DateOnly date, bool forThisWeek);
	}
}