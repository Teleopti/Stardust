using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakePersonFinderReadOnlyRepository : IPersonFinderReadOnlyRepository
	{
		private readonly IList<IPerson> _personList = new List<IPerson>();
		public void Has(IPerson person)
		{
			_personList.Add(person);
		}
		public void Find(IPersonFinderSearchCriteria personFinderSearchCriteria)
		{
			throw new NotImplementedException();
		}

		public void FindInTeams(IPersonFinderSearchCriteria personFinderSearchCriteria, Guid[] teamIds)
		{
			throw new NotImplementedException();
		}

		public void FindPeople(IPeoplePersonFinderSearchCriteria personFinderSearchCriteria)
		{
			throw new NotImplementedException();
		}

		public void UpdateFindPerson(ICollection<Guid> ids)
		{
			throw new NotImplementedException();
		}

		public void UpdateFindPersonData(ICollection<Guid> ids)
		{
			throw new NotImplementedException();
		}

		public List<Guid> FindPersonIdsInTeams(DateOnly date, Guid[] teamIds, IDictionary<PersonFinderField, string> searchCriteria)
		{
			throw new NotImplementedException();
		}

		public List<Guid> FindPersonIdsInTeamsBasedOnPersonPeriod(DateOnlyPeriod period, Guid[] teamIds, IDictionary<PersonFinderField, string> searchCriteria)
		{
			throw new NotImplementedException();
		}

		public List<Guid> FindPersonIdsInGroupsBasedOnPersonPeriod(DateOnlyPeriod period, Guid[] groupIds, IDictionary<PersonFinderField, string> searchCriteria)
		{
			return _personList.Select(p => p.Id.GetValueOrDefault()).ToList();
		}

		public List<Guid> FindPersonIdsInDynamicOptionalGroupPages(DateOnlyPeriod period, Guid groupPageId, string[] dynamicValues,
			IDictionary<PersonFinderField, string> searchCriteria)
		{
			return _personList.Select(p => p.Id.GetValueOrDefault()).ToList();
		}
	}
}