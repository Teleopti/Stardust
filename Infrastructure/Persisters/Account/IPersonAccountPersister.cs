using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Persisters.Account
{
	public interface IPersonAccountPersister
	{
		bool Persist(ICollection<IPersonAbsenceAccount> personAbsenceAccounts);
	}
}