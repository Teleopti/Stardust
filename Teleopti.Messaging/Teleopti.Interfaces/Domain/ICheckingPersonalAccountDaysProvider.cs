
using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
	public interface ICheckingPersonalAccountDaysProvider
	{
		/// <remarks>To be more efficient, we only return the first day for each personal account for NewPersonAccountRule</remarks>
		IEnumerable<DateOnly> GetDays(IAbsence absence, IPerson person, DateTimePeriod period);
	}
}
