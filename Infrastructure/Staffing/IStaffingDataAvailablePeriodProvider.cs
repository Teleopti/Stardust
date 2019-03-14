using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Infrastructure.Staffing
{
	public interface IStaffingDataAvailablePeriodProvider
	{
		DateOnlyPeriod? GetPeriodForAbsenceForWeek(IPerson person, DateOnly date);
		DateOnlyPeriod? GetPeriodForAbsenceForMobileDay(IPerson person, DateOnly date);
		List<DateOnlyPeriod> GetPeriodsForOvertimeForWeek(IPerson person, DateOnly date);
		List<DateOnlyPeriod> GetPeriodsForOvertimeForMobileDay(IPerson person, DateOnly date);
	}
}