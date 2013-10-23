using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Persisters.Account
{
	public class PersonAccountConflictCollector : IPersonAccountConflictCollector
	{
		private readonly ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;

		public PersonAccountConflictCollector(ICurrentUnitOfWorkFactory currentUnitOfWorkFactory)
		{
			_currentUnitOfWorkFactory = currentUnitOfWorkFactory;
		}

		public IEnumerable<IPersonAbsenceAccount> GetConflicts(IEnumerable<IPersonAbsenceAccount> personAbsenceAccounts)
		{
			var unitOfWork = _currentUnitOfWorkFactory.LoggedOnUnitOfWorkFactory().CurrentUnitOfWork();
			return (from e in personAbsenceAccounts
							let databaseVersion = unitOfWork.DatabaseVersion(e)
							where e.Version != databaseVersion
							select e);
		}
	}
}