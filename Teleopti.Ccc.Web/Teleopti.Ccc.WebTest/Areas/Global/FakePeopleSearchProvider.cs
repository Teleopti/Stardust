using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teleopti.Ccc.Web.Areas.People.Core.Providers;
using Teleopti.Ccc.Web.Areas.People.Core.ViewModels;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Global
{
	public class FakePeopleSearchProvider : IPeopleSearchProvider
	{
		private IEnumerable<IPerson> _persons = new List<IPerson>(); 

		public PeopleSummaryModel SearchPermittedPeople(IDictionary<PersonFinderField, string> criteriaDictionary, int pageSize, int currentPageIndex,
			DateOnly currentDate, IDictionary<string, bool> sortedColumns, string function)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<IPerson> SearchPermittedPeople(IDictionary<PersonFinderField, string> criteriaDictionary, DateOnly dateInUserTimeZone, string function)
		{
			return _persons;
		}

		public IEnumerable<Guid> GetPermittedPersonIdList(IDictionary<PersonFinderField, string> criteriaDictionary, int pageSize, int currentPageIndex,
			DateOnly currentDate, IDictionary<string, bool> sortedColumns, string function)
		{
			throw new NotImplementedException();
		}

		public void PresetReturnPeople(IEnumerable<IPerson> persons)
		{
			_persons = persons;
		}
	}
}
