using System.Collections.Generic;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Persisters
{
	public interface IPersonAbsenceAccountConflictCollector
	{
		IEnumerable<IPersonAbsenceAccount> GetConflicts(IUnitOfWork unitOfWork,
		                                                IEnumerable<IPersonAbsenceAccount> personAbsenceAccounts);
	}
}