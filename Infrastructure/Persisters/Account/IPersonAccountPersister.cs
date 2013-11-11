using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Persisters.Account
{
	public interface IPersonAccountPersister
	{
		void Persist(ICollection<IPersonAbsenceAccount> personAbsenceAccounts);
	}
}