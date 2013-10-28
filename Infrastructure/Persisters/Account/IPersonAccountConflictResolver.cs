using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Persisters.Account
{
	public interface IPersonAccountConflictResolver
	{
		void Resolve(IEnumerable<IPersonAbsenceAccount> conflictingPersonAccounts);
	}
}