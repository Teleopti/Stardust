using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Persisters.Account
{
	public interface IPersonAccountConflictCollector
	{
		IEnumerable<IPersonAbsenceAccount> GetConflicts(IEnumerable<IPersonAbsenceAccount> personAbsenceAccounts); 
	}
}