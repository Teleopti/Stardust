using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Ccc.Web.Areas.People.Core.ViewModels;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.People.Core.Providers
{
	public class PeopleSearchProvider : IPeopleSearchProvider
	{
		private readonly IPersonFinderReadOnlyRepository _searchRepository;
		private readonly IPersonRepository _personRepository;
		private readonly IPermissionProvider _permissionProvider;
		private readonly IOptionalColumnRepository _optionalColumnRepository;
		private readonly ILoggedOnUser _loggonUser;

		public PeopleSearchProvider(IPersonFinderReadOnlyRepository searchRepository,
			IPersonRepository personRepository,
			IPermissionProvider permissionProvider,
			IOptionalColumnRepository optionalColumnRepository,
			ILoggedOnUser loggonUser)
		{
			_searchRepository = searchRepository;
			_personRepository = personRepository;
			_permissionProvider = permissionProvider;
			_optionalColumnRepository = optionalColumnRepository;
			_loggonUser = loggonUser;
		}

		public PeopleSummaryModel SearchPeople(string keyword, int pageSize, int currentPageIndex, DateOnly currentDate)
		{
			var myTeam = _loggonUser.CurrentUser().MyTeam(currentDate);
			var optionalColumnCollection = _optionalColumnRepository.GetOptionalColumns<Person>();
			var searchType = PersonFinderField.All;

			if (string.IsNullOrEmpty(keyword) && myTeam != null)
			{
				keyword = myTeam.Description.Name;
				searchType = PersonFinderField.Organization;
			}

			var search = new PersonFinderSearchCriteria(searchType, keyword, pageSize, currentDate, 1, 1)
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