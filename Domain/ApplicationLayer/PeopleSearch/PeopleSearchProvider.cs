using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.People.Core.ViewModels;

namespace Teleopti.Ccc.Domain.ApplicationLayer.PeopleSearch
{
	public class PeopleSearchProvider : IPeopleSearchProvider
	{
		private readonly IPersonFinderReadOnlyRepository _searchRepository;
		private readonly IPersonRepository _personRepository;
		private readonly IPermissionProvider _permissionProvider;
		private readonly IOptionalColumnRepository _optionalColumnRepository;
		private readonly ICurrentBusinessUnit _businessUnitProvider;
		private readonly ILoggedOnUser _loggedOnUser;

		public PeopleSearchProvider(
			IPersonFinderReadOnlyRepository searchRepository,
			IPersonRepository personRepository,
			IPermissionProvider permissionProvider,
			IOptionalColumnRepository optionalColumnRepository,
			ICurrentBusinessUnit businessUnitProvider,
			ILoggedOnUser loggedOnUser)
		{
			_searchRepository = searchRepository;
			_personRepository = personRepository;
			_permissionProvider = permissionProvider;
			_optionalColumnRepository = optionalColumnRepository;
			_businessUnitProvider = businessUnitProvider;
			_loggedOnUser = loggedOnUser;
		}

		public PeopleSummaryModel SearchPermittedPeopleSummary(IDictionary<PersonFinderField, string> criteriaDictionary,
			int pageSize, int currentPageIndex, DateOnly currentDate, IDictionary<string, bool> sortedColumns, string function)
		{
			var personIdList = getPermittedPersonIdList(criteriaDictionary, 9999, 1, currentDate,
				sortedColumns, function);

			return searchPermittedPeopleSummary(personIdList, pageSize, currentPageIndex);
		}

		public List<Guid> FindPersonIds(DateOnly date, Guid[] teamIds, IDictionary<PersonFinderField, string> searchCriteria)
		{
			return _searchRepository.FindPersonIdsInTeams(date, teamIds, searchCriteria);
		}

		public List<Guid> FindPersonIdsInPeriodWithGroup(DateOnlyPeriod period, Guid[] groupIds,
			IDictionary<PersonFinderField, string> searchCriteria)
		{

			return _searchRepository.FindPersonIdsInGroupsBasedOnPersonPeriod(period, groupIds, searchCriteria);
		}


		public List<Guid> FindPersonIdsInPeriodWithDynamicGroup(DateOnlyPeriod period, Guid groupPageId, string[] dynamicValues, IDictionary<PersonFinderField, string> searchCriteria)
		{
			return _searchRepository.FindPersonIdsInDynamicOptionalGroupPages(period, groupPageId, dynamicValues, searchCriteria);
		}

		public List<IPerson> SearchPermittedPeople(PersonFinderSearchCriteria searchCriteria,
			DateOnly dateInUserTimeZone, string function)
		{
			var personIdList = getPermittedPersonIdList(searchCriteria, dateInUserTimeZone, function);
			return _personRepository.FindPeople(personIdList).ToList();
		}

		private List<Guid> getPermittedPersonIdList(PersonFinderSearchCriteria searchCriteria, DateOnly currentDate,
			string function)
		{
			var permittedPersonList =
				searchCriteria.DisplayRows.Where(
					r => r.RowNumber > 0 && _permissionProvider.HasOrganisationDetailPermission(function, currentDate, r));
			return permittedPersonList
				.Select(x => x.PersonId)
				.ToList();
		}

		public List<Guid> GetPermittedPersonIdList(IEnumerable<IPerson> people, DateOnly currentDate,
			string function)
		{
			return GetPermittedPersonList(people, currentDate, function)
				.Select(x => x.Id.GetValueOrDefault())
				.ToList();
		}

		public List<IPerson> GetPermittedPersonList(IEnumerable<IPerson> people, DateOnly date, string function)
		{
			var peopleByTeam = people.ToList().GroupBy(p => p.MyTeam(date));

			var permittedPeople = peopleByTeam
				.Where(team => team.Key != null && _permissionProvider.HasTeamPermission(function, date, team.Key))
				.SelectMany(gp => gp.ToList())
				.ToList() ?? new List<IPerson>();

			var uncheckedPeople = peopleByTeam.Where(pg => pg.Key == null).SelectMany(gp => gp.ToList());
			if (uncheckedPeople.Any())
			{
				permittedPeople.AddRange(uncheckedPeople.Where(p => checkIfPersonHasOrganisationDetailPermission(function, p, date)));
			}

			var loggedOnUser = _loggedOnUser.CurrentUser();
			var loggedOnUserId = loggedOnUser.Id;

			if (people.Any(p => p.Id == loggedOnUserId)
				&& !permittedPeople.Any(p => p.Id == loggedOnUserId)
				&& checkIfPersonHasOrganisationDetailPermission(function, loggedOnUser, date))
			{
				permittedPeople.Add(loggedOnUser);
			}


			return permittedPeople;
		}


		private bool checkIfPersonHasOrganisationDetailPermission(string function, IPerson p, DateOnly date)
		{
			var businessUnitId = _businessUnitProvider.Current().Id.GetValueOrDefault();
			var myTeam = p.MyTeam(date);
			return _permissionProvider.HasOrganisationDetailPermission(function, date, new PersonFinderDisplayRow
			{
				PersonId = p.Id.GetValueOrDefault(),
				TeamId = myTeam?.Id,
				SiteId = myTeam?.Site.Id,
				BusinessUnitId = businessUnitId
			});
		}

		public PersonFinderSearchCriteria CreatePersonFinderSearchCriteria(IDictionary<PersonFinderField, string> criteriaDictionary,
			int pageSize, int currentPageIndex, DateOnly currentDate, IDictionary<string, bool> sortedColumns)
		{
			var search = new PersonFinderSearchCriteria(criteriaDictionary, pageSize, currentDate, sortedColumns, currentDate)
			{
				CurrentPage = currentPageIndex
			};

			return search;
		}


		public void PopulateSearchCriteriaResult(PersonFinderSearchCriteria search)
		{
			_searchRepository.Find(search);
		}



		private IEnumerable<Guid> getPermittedPersonIdList(IDictionary<PersonFinderField, string> criteriaDictionary,
			int pageSize, int currentPageIndex, DateOnly currentDate, IDictionary<string, bool> sortedColumns, string function)
		{
			var search = CreatePersonFinderSearchCriteria(criteriaDictionary, pageSize, currentPageIndex, currentDate, sortedColumns);
			PopulateSearchCriteriaResult(search);
			var permittedPersonList =
				search.DisplayRows.Where(
					r => r.RowNumber > 0 && _permissionProvider.HasOrganisationDetailPermission(function, currentDate, r));
			return permittedPersonList.Select(x => x.PersonId);
		}

		private PeopleSummaryModel searchPermittedPeopleSummary(IEnumerable<Guid> personIds, int pageSize, int currentPageIndex)
		{
			var optionalColumnCollection = _optionalColumnRepository.GetOptionalColumns<Person>();
			var peopleList = _personRepository.FindPeople(personIds).ToList();
			var totalCount = peopleList.Count;
			var totalPages = totalCount / pageSize;
			if (totalCount % pageSize > 0)
			{
				totalPages++;
			}
			peopleList = peopleList.Skip((currentPageIndex - 1) * pageSize).Take(pageSize).ToList();

			return new PeopleSummaryModel
			{
				People = peopleList,
				TotalPages = totalPages,
				OptionalColumns = optionalColumnCollection
			};
		}

	}
}
