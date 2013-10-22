using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Persisters.Account
{
	public class PersonAccountConflictCollector : IPersonAccountConflictCollector
	{
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;

		public PersonAccountConflictCollector(IUnitOfWorkFactory unitOfWorkFactory)
		{
			_unitOfWorkFactory = unitOfWorkFactory;
		}

		public IEnumerable<IPersonAbsenceAccount> GetConflicts(IEnumerable<IPersonAbsenceAccount> personAbsenceAccounts)
		{
			var unitOfWork = _unitOfWorkFactory.CurrentUnitOfWork();
			return (from e in personAbsenceAccounts
							let databaseVersion = unitOfWork.DatabaseVersion(e)
							where e.Version != databaseVersion
							select e);
		}
	}
}