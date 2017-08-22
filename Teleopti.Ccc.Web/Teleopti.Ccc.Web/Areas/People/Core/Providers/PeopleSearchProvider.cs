using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
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
		private readonly IPersonAbsenceRepository _personAbsenceRepository;
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly ICurrentBusinessUnit _businessUnitProvider;
		private readonly ICurrentScenario _currentScenario;

		public PeopleSearchProvider(
			IPersonFinderReadOnlyRepository searchRepository,
			IPersonRepository personRepository,
			IPermissionProvider permissionProvider,
			IOptionalColumnRepository optionalColumnRepository, IPersonAbsenceRepository personAbsenceRepository,
			ILoggedOnUser loggedOnUser,
			ICurrentBusinessUnit businessUnitProvider,
			ICurrentScenario currentScenario)
		{
			_searchRepository = searchRepository;
			_personRepository = personRepository;
			_permissionProvider = permissionProvider;
			_optionalColumnRepository = optionalColumnRepository;
			_personAbsenceRepository = personAbsenceRepository;
			_loggedOnUser = loggedOnUser;
			_businessUnitProvider = businessUnitProvider;
			_currentScenario = currentScenario;
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

		public List<Guid> FindPersonIdsInPeriod(DateOnlyPeriod period, Guid[] teamIds,
			IDictionary<PersonFinderField, string> searchCriteria)
		{
			return _searchRepository.FindPersonIdsInTeamsBasedOnPersonPeriod(period, teamIds, searchCriteria);
		}

		public List<Guid> FindPersonIdsInPeriodWithGroup(DateOnlyPeriod period, Guid[] groupIds,
			IDictionary<PersonFinderField, string> searchCriteria, string[] dynamicValues)
		{
			if (groupIds.Any())
			{
				return _searchRepository.FindPersonIdsInGroupsBasedOnPersonPeriod(period, groupIds, searchCriteria);
			}
			return _searchRepository.FindPersonIdsInDynamicOptionalGroupPages(period, dynamicValues, searchCriteria);
		}

		public IEnumerable<IPerson> SearchPermittedPeople(IDictionary<PersonFinderField, string> criteriaDictionary,
			DateOnly dateInUserTimeZone, string function)
		{
			var searchCriteria = CreatePersonFinderSearchCriteria(criteriaDictionary, 9999, 1, dateInUserTimeZone,
				null);
			PopulateSearchCriteriaResult(searchCriteria);
			var personIdList = GetPermittedPersonIdList(searchCriteria, dateInUserTimeZone, function);
			return _personRepository.FindPeople(personIdList).ToList();
		}

		public IEnumerable<IPerson> SearchPermittedPeople(PersonFinderSearchCriteria searchCriteria,
			DateOnly dateInUserTimeZone, string function)
		{
			var personIdList = GetPermittedPersonIdList(searchCriteria, dateInUserTimeZone, function);
			return _personRepository.FindPeople(personIdList).ToList();
		}

		public IEnumerable<IPerson> SearchPermittedPeopleWithAbsence(IEnumerable<IPerson> permittedPeople,
			DateOnly dateInUserTimeZone)
		{
			var dateTimePeriod = dateInUserTimeZone.ToDateTimePeriod(_loggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone()).ChangeEndTime(TimeSpan.FromSeconds(-1));
			var personAbsences =
				_personAbsenceRepository.Find(permittedPeople, dateTimePeriod, _currentScenario.Current()).ToList();

			return personAbsences.Select(personAbsence => personAbsence.Person).ToList();
		}

		public IEnumerable<Guid> GetPermittedPersonIdList(PersonFinderSearchCriteria searchCriteria, DateOnly currentDate,
			string function)
		{
			var permittedPersonList =
				searchCriteria.DisplayRows.Where(
					r => r.RowNumber > 0 && _permissionProvider.HasOrganisationDetailPermission(function, currentDate, r));
			return permittedPersonList.Select(x => x.PersonId);
		}

		public IEnumerable<Guid> GetPermittedPersonIdList(IEnumerable<IPerson> people, DateOnly currentDate,
			string function)
		{
			return GetPermittedPersonList(people, currentDate, function).Select(x => x.Id.GetValueOrDefault());
		}

		public IEnumerable<IPerson> GetPermittedPersonList(IEnumerable<IPerson> people, DateOnly currentDate,
			string function)
		{
			return
				people.Where(p =>
				{
					var myTeam = p.MyTeam(currentDate);
					return _permissionProvider.HasOrganisationDetailPermission(function, currentDate, new PersonFinderDisplayRow
					{
						PersonId = p.Id.GetValueOrDefault(),
						TeamId = myTeam?.Id,
						SiteId = myTeam?.Site.Id,
						BusinessUnitId = _businessUnitProvider.Current().Id.GetValueOrDefault()
					});
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

		public void PopulateSearchCriteriaResult(PersonFinderSearchCriteria search, Guid[] teamIds)
		{
			_searchRepository.FindInTeams(search, teamIds);
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
