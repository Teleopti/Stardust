﻿using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.People.Core.ViewModels;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.PeopleSearch
{
	public interface IPeopleSearchProvider
	{
		PeopleSummaryModel SearchPermittedPeopleSummary(IDictionary<PersonFinderField, string> criteriaDictionary,
			int pageSize, int currentPageIndex, DateOnly currentDate, IDictionary<string, bool> sortedColumns, string function);
		
		IEnumerable<IPerson> SearchPermittedPeople(PersonFinderSearchCriteria searchCriteria, DateOnly dateInUserTimeZone,
			string function);
		
		IEnumerable<IPerson> SearchPermittedPeopleWithAbsence(IEnumerable<IPerson> permittedPeople, DateOnly dateInUserTimeZone);

		PersonFinderSearchCriteria CreatePersonFinderSearchCriteria(IDictionary<PersonFinderField, string> criteriaDictionary,
			int pageSize, int currentPageIndex, DateOnly currentDate, IDictionary<string, bool> sortedColumns);

		IEnumerable<Guid> GetPermittedPersonIdList(IEnumerable<IPerson> people, DateOnly currentDate,
			string function);

		IEnumerable<IPerson> GetPermittedPersonList(IEnumerable<IPerson> people, DateOnly currentDate,
			string function);

		void PopulateSearchCriteriaResult(PersonFinderSearchCriteria search);
		void PopulateSearchCriteriaResult(PersonFinderSearchCriteria search, Guid[] teamIds);

		List<Guid> FindPersonIds(DateOnly date, Guid[] teamIds, IDictionary<PersonFinderField, string> searchCriteria);

		List<Guid> FindPersonIdsInPeriod(DateOnlyPeriod period, Guid[] teamIds,
			IDictionary<PersonFinderField, string> searchCriteria);

		List<Guid> FindPersonIdsInPeriodWithGroup(DateOnlyPeriod period, Guid[] groupIds,
			IDictionary<PersonFinderField, string> searchCriteria);

		List<Guid> FindPersonIdsInPeriodWithDynamicGroup(DateOnlyPeriod period, Guid groupPageId, string[] dynamicValues,
			IDictionary<PersonFinderField, string> searchCriteria);
	}
}
