using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Castle.Core.Internal;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
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
		private readonly ITeamRepository _teamRepo;

		public PeopleSearchProvider(
			IPersonFinderReadOnlyRepository searchRepository,
			IPersonRepository personRepository,
			IPermissionProvider permissionProvider,
			IOptionalColumnRepository optionalColumnRepository, IPersonAbsenceRepository personAbsenceRepository,
			ILoggedOnUser loggedOnUser, ICurrentBusinessUnit businessUnitProvider, ICurrentScenario currentScenario, ITeamRepository teamRepo)
		{
			_searchRepository = searchRepository;
			_personRepository = personRepository;
			_permissionProvider = permissionProvider;
			_optionalColumnRepository = optionalColumnRepository;
			_personAbsenceRepository = personAbsenceRepository;
			_loggedOnUser = loggedOnUser;
			_businessUnitProvider = businessUnitProvider;
			_currentScenario = currentScenario;
			_teamRepo = teamRepo;
		}

		public PeopleSummaryModel SearchPermittedPeopleSummary(IDictionary<PersonFinderField, string> criteriaDictionary,
			int pageSize, int currentPageIndex, DateOnly currentDate, IDictionary<string, bool> sortedColumns, string function)
		{
			var personIdList = getPermittedPersonIdList(criteriaDictionary, 9999, 1, currentDate,
				sortedColumns, function);

			return searchPermittedPeopleSummary(personIdList, pageSize, currentPageIndex);
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

		public IEnumerable<Guid> GetPermittedPersonIdList(IEnumerable<IPerson> people ,DateOnly currentDate,
			string function)
		{
			return GetPermittedPersonList(people, currentDate, function).Select(x => x.Id.GetValueOrDefault());			
		}

		public IEnumerable<IPerson> GetPermittedPersonList(IEnumerable<IPerson> people,DateOnly currentDate,
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

		public IEnumerable<IPerson> SearchPermittedPeopleWithinTeams(Guid[] selectedTeamIds,
			IDictionary<PersonFinderField, string> criteriaDictionary,
			DateOnly dateInUserTimeZone)
		{
			var teams = _teamRepo.FindTeams(selectedTeamIds);
			var peopleInTeams = new List<IPerson>();
			foreach (var team in teams)
			{
				peopleInTeams.AddRange(_personRepository.FindPeopleBelongTeam(team, new DateOnlyPeriod(dateInUserTimeZone, dateInUserTimeZone)).Where(p => p.IsAgent(dateInUserTimeZone)));
			}
			var matchedPeople = new List<IPerson>();
			foreach (var person in peopleInTeams)
			{
				if(criteriaDictionary.All(pair => isPersonMatched(person, dateInUserTimeZone, pair.Key, pair.Value)))
				{
					matchedPeople.Add(person);
				}
			}
			return matchedPeople.Where(
				p =>
						_permissionProvider.HasPersonPermission(DefinedRaptorApplicationFunctionPaths.ViewSchedules, dateInUserTimeZone, p));
		}

		private bool isPersonMatched(IPerson person, DateOnly date, PersonFinderField field, string value)
		{
			var personPeriods = person.PersonPeriods(new DateOnlyPeriod(date, date));
			var terms = parse(value);
			switch (field)
			{
				case PersonFinderField.FirstName:
					return terms.Any(t => person.Name.FirstName.ContainsIgnoreCase(t));
				case PersonFinderField.LastName:
					return terms.Any(t => person.Name.LastName.ContainsIgnoreCase(t));
				case PersonFinderField.Note:
					return terms.Any(t => person.Note.ContainsIgnoreCase(t));
				case PersonFinderField.BudgetGroup:
					return terms.Any(t => personPeriods.Any(p => p.BudgetGroup != null && p.BudgetGroup.Name.ContainsIgnoreCase(t)));
				case PersonFinderField.Contract:
					return terms.Any(t => personPeriods.Any(p => p.PersonContract != null && p.PersonContract.Contract.Description.Name.ContainsIgnoreCase(t)));
				case PersonFinderField.ContractSchedule:
					return terms.Any(t => personPeriods.Any(p => p.PersonContract.ContractSchedule.Description.Name.ContainsIgnoreCase(t)));
				case PersonFinderField.Organization:
					return terms.Any(t => personPeriods.Any(p => p.Team.SiteAndTeam.ContainsIgnoreCase(t)));
				case PersonFinderField.PartTimePercentage:
					return terms.Any(t => personPeriods.Any(p => p.PersonContract != null && p.PersonContract.PartTimePercentage.Description.Name.ContainsIgnoreCase(
						t)));
				case PersonFinderField.ShiftBag:
					return terms.Any(t => personPeriods.Any(p => p.RuleSetBag !=null &&p.RuleSetBag.Description.Name.ContainsIgnoreCase(t)));
				case PersonFinderField.Skill:
					return terms.Any(t => personPeriods.Any(p => p.PersonSkillCollection.Any(s => s.Skill.Name.ContainsIgnoreCase(t))));
				case PersonFinderField.Role:
					return terms.Any(t => person.PermissionInformation.ApplicationRoleCollection.Any(r => r.Name.ContainsIgnoreCase(t)));
				default:
					return terms.Any(t => person.Name.FirstName.ContainsIgnoreCase(t))
						   || terms.Any(t => person.Name.LastName.ContainsIgnoreCase(t))
						   || terms.Any(t => person.Note.ContainsIgnoreCase(t))
						   || terms.Any(t => personPeriods.Any(p => p.BudgetGroup != null && p.BudgetGroup.Name.ContainsIgnoreCase(t)))
						   || terms.Any(t => personPeriods.Any(p => p.PersonContract != null && p.PersonContract.Contract.Description.Name.ContainsIgnoreCase(t)))
						   || terms.Any(t => personPeriods.Any(p => p.PersonContract != null && p.PersonContract.ContractSchedule.Description.Name.ContainsIgnoreCase(t)))
						   || terms.Any(t => personPeriods.Any(p => p.Team.SiteAndTeam.ContainsIgnoreCase(t)))
						   || terms.Any(t => personPeriods.Any(p => p.PersonContract != null && p.PersonContract.PartTimePercentage.Description.Name.ContainsIgnoreCase(
						t)))
						   || terms.Any(t => personPeriods.Any(p => p.RuleSetBag != null && p.RuleSetBag.Description.Name.ContainsIgnoreCase(t)))
						   || terms.Any(t => personPeriods.Any(p => p.PersonSkillCollection.Any(s => s.Skill.Name.ContainsIgnoreCase(t))))
						   || terms.Any(t => person.PermissionInformation.ApplicationRoleCollection.Any(r => r.Name.ContainsIgnoreCase(t)));
			}

		}

		private IEnumerable<string> parse(string searchValue)
		{
			const string quotePattern = "(\"[^\"]*?\")";

			var notParsedSearchValue = Regex.Replace(searchValue, quotePattern, " $1 ");
			notParsedSearchValue = Regex.Replace(notParsedSearchValue, " {2,}", " ");

			const string splitPattern = "[^\\s\"]+|\"[^\"]*\"";
			var matches = Regex.Matches(notParsedSearchValue, splitPattern);
			var result =
				(from object match in matches select match.ToString().Replace("\"", "").Trim())
				.Where(x => !string.IsNullOrEmpty(x));

			return new HashSet<string>(result);
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
