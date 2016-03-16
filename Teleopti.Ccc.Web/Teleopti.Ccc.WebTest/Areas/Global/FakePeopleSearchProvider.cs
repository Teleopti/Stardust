using System;
using System.Collections.Generic;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Web.Areas.People.Core.Providers;
using Teleopti.Ccc.Web.Areas.People.Core.ViewModels;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Global
{
	public class FakePeopleSearchProvider : IPeopleSearchProvider
	{
		private IEnumerable<IPerson> _persons = new List<IPerson>();

		public PeopleSummaryModel SearchPermittedPeopleSummary(IDictionary<PersonFinderField, string> criteriaDictionary, int pageSize, int currentPageIndex,
			DateOnly currentDate, IDictionary<string, bool> sortedColumns, string function)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<IPerson> SearchPermittedPeople(IDictionary<PersonFinderField, string> criteriaDictionary,
			DateOnly dateInUserTimeZone, string function)
		{
			return _persons;
		}

		public IEnumerable<IPerson> SearchPermittedPeople(PersonFinderSearchCriteria searchCriteria, DateOnly dateInUserTimeZone,
			string function)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<Guid> GetPermittedPersonIdList(PersonFinderSearchCriteria searchCriteria, DateOnly currentDate, string function)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<IPerson> SearchPermittedPeopleWithAbsence(IEnumerable<IPerson> permittedPeople, DateOnly dateInUserTimeZone)
		{
			throw new NotImplementedException();
		}

		public PersonFinderSearchCriteria CreatePersonFinderSearchCriteria(IDictionary<PersonFinderField, string> criteriaDictionary, int pageSize,
			int currentPageIndex, DateOnly currentDate, IDictionary<string, bool> sortedColumns)
		{
			throw new NotImplementedException();
		}

		public void PresetReturnPeople(IEnumerable<IPerson> persons)
		{
			_persons = persons;
		}
	}
}
