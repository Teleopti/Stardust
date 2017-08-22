using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface IPersonFinderReadOnlyRepository
	{
		void Find(IPersonFinderSearchCriteria personFinderSearchCriteria);
		void FindInTeams(IPersonFinderSearchCriteria personFinderSearchCriteria, Guid[] teamIds);
		void FindPeople(IPeoplePersonFinderSearchCriteria personFinderSearchCriteria);
		void UpdateFindPerson(ICollection<Guid> ids);
		void UpdateFindPersonData(ICollection<Guid> ids);
		List<Guid> FindPersonIdsInTeams(DateOnly date, Guid[] teamIds, IDictionary<PersonFinderField, string> searchCriteria);

		List<Guid> FindPersonIdsInTeamsBasedOnPersonPeriod(DateOnlyPeriod period, Guid[] teamIds,
			IDictionary<PersonFinderField, string> searchCriteria);

		List<Guid> FindPersonIdsInGroupsBasedOnPersonPeriod(DateOnlyPeriod period, Guid[] groupIds,
			IDictionary<PersonFinderField, string> searchCriteria);

		List<Guid> FindPersonIdsInDynamicOptionalGroupPages(DateOnlyPeriod period,
			string[] dynamicValues, IDictionary<PersonFinderField, string> searchCriteria);
	}
}
