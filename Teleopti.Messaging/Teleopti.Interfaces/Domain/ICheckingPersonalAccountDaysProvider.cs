
using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
	public interface ICheckingPersonalAccountDaysProvider
	{
		/// <summary>
		/// Get days for checking account
		/// </summary>
		/// <remarks>To be more efficient, we only return the first day for each personal account for NewPersonAccountRule</remarks>
		/// <returns></returns>
		IEnumerable<DateOnly> GetDays(IAbsence absence, IPerson person, DateTimePeriod period);
	}
}
