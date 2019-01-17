using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.People.Core.ViewModels;

namespace Teleopti.Ccc.Domain.ApplicationLayer.PeopleSearch
{
	public interface IPeopleSearchProvider
	{
		PeopleSummaryModel SearchPermittedPeopleSummary(IDictionary<PersonFinderField, string> criteriaDictionary,
			int pageSize, int currentPageIndex, DateOnly currentDate, IDictionary<string, bool> sortedColumns, string function);
		
		List<IPerson> SearchPermittedPeople(PersonFinderSearchCriteria searchCriteria, DateOnly dateInUserTimeZone,
			string function);

		PersonFinderSearchCriteria CreatePersonFinderSearchCriteria(IDictionary<PersonFinderField, string> criteriaDictionary,
			int pageSize, int currentPageIndex, DateOnly currentDate, IDictionary<string, bool> sortedColumns);

		HashSet<Guid> GetPermittedPersonIdList(IEnumerable<IPerson> people, DateOnly currentDate,
			string function);

		List<IPerson> GetPermittedPersonList(IEnumerable<IPerson> people, DateOnly currentDate,
			string function);

		void PopulateSearchCriteriaResult(PersonFinderSearchCriteria search);

		List<Guid> FindPersonIds(DateOnly date, Guid[] teamIds, IDictionary<PersonFinderField, string> searchCriteria);

		List<Guid> FindPersonIdsInPeriodWithGroup(DateOnlyPeriod period, Guid[] groupIds,
			IDictionary<PersonFinderField, string> searchCriteria);

		List<Guid> FindPersonIdsInPeriodWithDynamicGroup(DateOnlyPeriod period, Guid groupPageId, string[] dynamicValues,
			IDictionary<PersonFinderField, string> searchCriteria);
	}
}
