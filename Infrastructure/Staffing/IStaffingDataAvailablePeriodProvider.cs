using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Infrastructure.Staffing
{
	public interface IStaffingDataAvailablePeriodProvider
	{
		DateOnlyPeriod? GetPeriodForAbsence(IPerson person, DateOnly date, bool forThisWeek);
		List<DateOnlyPeriod> GetPeriodsForOvertime(IPerson person, DateOnly date, bool forThisWeek = false);
	}
}