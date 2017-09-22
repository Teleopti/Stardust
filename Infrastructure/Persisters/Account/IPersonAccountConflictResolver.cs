using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Infrastructure.Persisters.Account
{
	public interface IPersonAccountConflictResolver
	{
		void Resolve(IEnumerable<IPersonAbsenceAccount> conflictingPersonAccounts);
	}
}