
using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
	public interface ICheckingPersonalAccountDaysProvider
	{
		IEnumerable<DateOnly> GetDays(IAbsence absence, IPerson person, DateTimePeriod period);
	}
}
