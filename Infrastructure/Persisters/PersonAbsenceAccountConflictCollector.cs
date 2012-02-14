using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Persisters
{
	public class PersonAbsenceAccountConflictCollector : IPersonAbsenceAccountConflictCollector
	{
		public IEnumerable<IPersonAbsenceAccount> GetConflicts(IUnitOfWork unitOfWork,
		                                                       IEnumerable<IPersonAbsenceAccount> personAbsenceAccounts)
		{
			return (from e in personAbsenceAccounts
			        let databaseVersion = unitOfWork.DatabaseVersion(e)
			        where e.Version != databaseVersion
			        select e).ToArray();
		}
	}
}