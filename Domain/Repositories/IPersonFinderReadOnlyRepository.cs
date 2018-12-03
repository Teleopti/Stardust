using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface IPersonFinderReadOnlyRepository
	{
		void Find(IPersonFinderSearchCriteria personFinderSearchCriteria);
		void FindInTeams(IPersonFinderSearchCriteria personFinderSearchCriteria, Guid[] teamIds);
		void FindPeople(IPeoplePersonFinderSearchCriteria personFinderSearchCriteria);
		void FindPeopleWithDataPermission(IPeoplePersonFinderSearchWithPermissionCriteria personFinderSearchCriteria);
		void UpdateFindPerson(ICollection<Guid> ids);
		void UpdateFindPersonData(ICollection<Guid> ids);
		List<Guid> FindPersonIdsInTeams(DateOnly date, Guid[] teamIds, IDictionary<PersonFinderField, string> searchCriteria);

		List<Guid> FindPersonIdsInTeamsBasedOnPersonPeriod(DateOnlyPeriod period, Guid[] teamIds,
			IDictionary<PersonFinderField, string> searchCriteria);

		List<Guid> FindPersonIdsInGroupsBasedOnPersonPeriod(DateOnlyPeriod period, Guid[] groupIds,
			IDictionary<PersonFinderField, string> searchCriteria);

		List<Guid> FindPersonIdsInDynamicOptionalGroupPages(DateOnlyPeriod period, Guid groupPageId,
			string[] dynamicValues, IDictionary<PersonFinderField, string> searchCriteria);

		IList<PersonIdentityMatchResult> FindPersonByIdentities(IEnumerable<string> identities);

		bool ValidatePersonIds(List<Guid> ids, DateOnly date, Guid userId, string appFuncForeginId);
	}
}
