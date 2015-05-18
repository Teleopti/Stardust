using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface IPersonFinderReadOnlyRepository
	{
		void Find(IPersonFinderSearchCriteria personFinderSearchCriteria);
		void FindPeople(IPeoplePersonFinderSearchCriteria personFinderSearchCriteria);
		void UpdateFindPerson(ICollection<Guid> ids);
		void UpdateFindPersonData(ICollection<Guid> ids);
	}
}
