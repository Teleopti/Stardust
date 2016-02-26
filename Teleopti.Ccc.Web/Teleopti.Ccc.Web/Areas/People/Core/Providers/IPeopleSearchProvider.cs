using System;
using System.Collections.Generic;
using Teleopti.Ccc.Web.Areas.People.Core.ViewModels;
using Teleopti.Interfaces.Domain;
namespace Teleopti.Ccc.Web.Areas.People.Core.Providers
{
	public interface IPeopleSearchProvider
	{
		PeopleSummaryModel SearchPermittedPeople(IDictionary<PersonFinderField, string> criteriaDictionary,
			int pageSize, int currentPageIndex, DateOnly currentDate, IDictionary<string, bool> sortedColumns, string function);

		IEnumerable<IPerson> SearchPermittedPeople(IDictionary<PersonFinderField, string> criteriaDictionary, DateOnly dateInUserTimeZone, string function);
		IEnumerable<IPerson> SearchPermittedPeopleWithAbsence(IDictionary<PersonFinderField, string> criteriaDictionary, DateOnly dateInUserTimeZone, string function);
		IEnumerable<Guid> GetPermittedPersonIdList(IDictionary<PersonFinderField, string> criteriaDictionary,
			int pageSize, int currentPageIndex, DateOnly currentDate, IDictionary<string, bool> sortedColumns, string function);
	}
}
