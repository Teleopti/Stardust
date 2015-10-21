using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Web.Areas.Global.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Ccc.Web.Areas.People.Core.Models;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.People.Core.Providers
{
	public class PeopleSearchProvider : IPeopleSearchProvider
	{
		private readonly IPersonFinderReadOnlyRepository _searchRepository;
		private readonly IPersonRepository _personRepository;
		private readonly IPermissionProvider _permissionProvider;
		private readonly IOptionalColumnRepository _optionalColumnRepository;

		public PeopleSearchProvider(
			IPersonFinderReadOnlyRepository searchRepository,
			IPersonRepository personRepository,
			IPermissionProvider permissionProvider,
			IOptionalColumnRepository optionalColumnRepository)
		{
			_searchRepository = searchRepository;
			_personRepository = personRepository;
			_permissionProvider = permissionProvider;
			_optionalColumnRepository = optionalColumnRepository;
		}

		public PeopleSummaryModel SearchPeople(IDictionary<PersonFinderField, string> criteriaDictionary,
			int pageSize, int currentPageIndex, DateOnly currentDate, IDictionary<string, bool> sortedColumns)
		{
			var optionalColumnCollection = _optionalColumnRepository.GetOptionalColumns<Person>();

			var search = new PersonFinderSearchCriteria(criteriaDictionary, pageSize, currentDate, sortedColumns)
			{
				CurrentPage = currentPageIndex
			};

			_searchRepository.Find(search);

			var permittedPersonList =
				search.DisplayRows.Where(
					r =>
						r.RowNumber > 0 &&
						_permissionProvider.HasOrganisationDetailPermission(
						DefinedRaptorApplicationFunctionPaths.OpenPersonAdminPage,
							DateOnly.Today, r));
			var personIdList = permittedPersonList.Select(x => x.PersonId);
			var peopleList = _personRepository.FindPeople(personIdList).ToList();

			return new PeopleSummaryModel
			{
				People = peopleList,
				TotalPages = search.TotalPages,
				OptionalColumns = optionalColumnCollection
			};
		}
	}
}