using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Persisters.Account
{
	public class PersonAccountConflictCollector : IPersonAccountConflictCollector
	{
		private readonly DatabaseVersion _databaseVersion;

		public PersonAccountConflictCollector(DatabaseVersion databaseVersion)
		{
			_databaseVersion = databaseVersion;
		}

		public IEnumerable<IPersonAbsenceAccount> GetConflicts(IEnumerable<IPersonAbsenceAccount> personAbsenceAccounts)
		{
			return (from e in personAbsenceAccounts
							let databaseVersion = _databaseVersion.FetchFor(e, true)
							where e.Version != databaseVersion
							select e);
		}
	}
}