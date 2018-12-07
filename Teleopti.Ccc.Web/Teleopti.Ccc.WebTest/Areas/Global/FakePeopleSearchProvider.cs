using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Teleopti.Ccc.Domain.ApplicationLayer.PeopleSearch;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Areas.People.Core.ViewModels;


namespace Teleopti.Ccc.WebTest.Areas.Global
{
	public class FakePeopleSearchProvider : IPeopleSearchProvider
	{
		private readonly PeopleSummaryModel _model;
		private readonly List<IPerson> _permittedPeople;
		private readonly List<IPerson> _peopleWithConfidentialAbsencePermission;
		private readonly IDictionary<DateOnly, List<IPerson>> _permittedPeopleByDate;
		private bool _enableDateFilter;
		private readonly IDictionary<IPerson, string> _personApplicationRoleDictionary;
		private readonly Dictionary<Guid, List<IPerson>> _permittedPeopleInTeam = new Dictionary<Guid, List<IPerson>>();
		private bool _enablePermittedPeopleInTeams = false;


		const string quotePattern = "(?!\")[^\"]*?(?=\")";

		public FakePeopleSearchProvider(IEnumerable<IPerson> peopleList, IEnumerable<IOptionalColumn> optionalColumns)
		{
			_permittedPeople = new List<IPerson>();
			_peopleWithConfidentialAbsencePermission = new List<IPerson>();
			_permittedPeopleByDate = new Dictionary<DateOnly, List<IPerson>>();
			_model = new PeopleSummaryModel
			{
				People = peopleList.ToList(),
				OptionalColumns = optionalColumns.ToList()
			};
			_personApplicationRoleDictionary = new Dictionary<IPerson, string>();
		}

		public void EnableDateFilter()
		{
			_enableDateFilter = true;
		}

		public void EnablePermittedPeopleInTeams()
		{
			_enablePermittedPeopleInTeams = true;
		}

		public PeopleSummaryModel SearchPermittedPeopleSummary(IDictionary<PersonFinderField, string> criteriaDictionary,
			int pageSize, int currentPageIndex,
			DateOnly currentDate, IDictionary<string, bool> sortedColumns, string function)
		{
			return _model;
		}

		public List<IPerson> SearchPermittedPeople(PersonFinderSearchCriteria searchCriteria,
			DateOnly dateInUserTimeZone, string function)
		{
			if (_enableDateFilter)
			{
				return !_permittedPeopleByDate.ContainsKey(dateInUserTimeZone) ? new List<IPerson>() : _permittedPeopleByDate[dateInUserTimeZone].ToList();
			}
			return function == DefinedRaptorApplicationFunctionPaths.ViewConfidential
				? _peopleWithConfidentialAbsencePermission
				: _permittedPeople;
		}

		public PersonFinderSearchCriteria CreatePersonFinderSearchCriteria(
			IDictionary<PersonFinderField, string> criteriaDictionary, int pageSize,
			int currentPageIndex, DateOnly currentDate, IDictionary<string, bool> sortedColumns)
		{
			return new PersonFinderSearchCriteria(criteriaDictionary, pageSize, currentDate, sortedColumns, currentDate);
		}

		public List<Guid> GetPermittedPersonIdList(IEnumerable<IPerson> people, DateOnly currentDate, string function)
		{
			return GetPermittedPersonList(people, currentDate, function).Select(p => p.Id.GetValueOrDefault()).ToList();
		}

		public List<IPerson> GetPermittedPersonList(IEnumerable<IPerson> people, DateOnly currentDate, string function)
		{
			if (_enableDateFilter)
			{
				return !_permittedPeopleByDate.ContainsKey(currentDate) ? new List<IPerson>() : _permittedPeopleByDate[currentDate].ToList();
			}
			return function == DefinedRaptorApplicationFunctionPaths.ViewConfidential
				? _peopleWithConfidentialAbsencePermission
				: _permittedPeople;
		}

		public void PopulateSearchCriteriaResult(PersonFinderSearchCriteria search)
		{
			var people = new List<IPerson>();
			var date = search.BelongsToDate;
			if (_enableDateFilter)
			{
				people = !_permittedPeopleByDate.ContainsKey(date) ? new List<IPerson>() : _permittedPeopleByDate[date].ToList();
			}
			else
			{
				people = _permittedPeople.ToList();
			}

			search.TotalRows = people.Count;
			int row = 0;
			people.ForEach(p =>
			{
				search.SetRow(row, toPersonFinderDisplayRow(p, date, row + 1));
				row++;
			});
		}

		public List<Guid> FindPersonIds(DateOnly date, Guid[] teamIds, IDictionary<PersonFinderField, string> searchCriteria)
		{
			IEnumerable<IPerson> people;
			if (_enableDateFilter)
			{
				people = !_permittedPeopleByDate.ContainsKey(date) ? new List<IPerson>() : _permittedPeopleByDate[date];
			}
			else
			{
				people = _permittedPeople;
			}
			people = people.Where(p => p.PersonPeriodCollection.Any(pp => teamIds.ToList().Contains(pp.Team.Id.Value)));

			return people.Select(p => p.Id.Value).ToList();
		}



		private PersonFinderDisplayRow toPersonFinderDisplayRow(IPerson p, DateOnly date, int rowNumber)
		{
			var team = p.MyTeam(date);
			var site = team?.Site;
			var bu = site?.BusinessUnit;

			return new PersonFinderDisplayRow
			{
				PersonId = p.Id.GetValueOrDefault(),
				TeamId = team?.Id,
				SiteId = site?.Id,
				BusinessUnitId = bu?.Id ?? Guid.Empty,
				RowNumber = rowNumber
			};
		}

		public void Add(IPerson person, string roleDescription = null)
		{
			_permittedPeople.Add(person);
			if (roleDescription != null)
			{
				_personApplicationRoleDictionary.Add(person, roleDescription);
			}
		}

		public void Add(DateOnly date, params IPerson[] persons)
		{
			if (_permittedPeopleByDate.ContainsKey(date))
			{
				_permittedPeopleByDate[date].AddRange(persons);
			}
			else
			{
				_permittedPeopleByDate[date] = persons.ToList();
			}
		}

		public void Add(Guid teamId, IPerson person)
		{
			if (!_permittedPeopleInTeam.ContainsKey(teamId))
			{
				_permittedPeopleInTeam.Add(teamId, new List<IPerson>());
			}
			_permittedPeopleInTeam[teamId].Add(person);
		}

		public void AddPersonWithViewConfidentialPermission(IPerson person)
		{
			_peopleWithConfidentialAbsencePermission.Add(person);
		}

		public List<Guid> FindPersonIdsInPeriodWithGroup(DateOnlyPeriod period, Guid[] groupIds, IDictionary<PersonFinderField, string> searchCriteria)
		{
			if (_enablePermittedPeopleInTeams)
			{
				return _permittedPeopleInTeam.Where(p => groupIds.Contains(p.Key)).SelectMany(pit => pit.Value.Select(p => p.Id.Value)).ToList();
			}

			IEnumerable<IPerson> people = _permittedPeople;

			if (searchCriteria != null && searchCriteria.ContainsKey(PersonFinderField.Role))
			{
				people = filterByRole(searchCriteria, people);
			}

			return people.Select(p => p.Id.Value).ToList();
		}

		public List<Guid> FindPersonIdsInPeriodWithDynamicGroup(DateOnlyPeriod period, Guid groupPageId, string[] dynamicValues, IDictionary<PersonFinderField, string> searchCriteria)
		{
			IEnumerable<IPerson> people = _permittedPeople;

			if (searchCriteria.ContainsKey(PersonFinderField.Role))
			{
				people = filterByRole(searchCriteria, people);
			}

			if (searchCriteria.ContainsKey(PersonFinderField.FirstName))
			{
				people = filterByFirstName(searchCriteria, people);
			}

			return people.Select(p => p.Id.Value).ToList();
		}

		private IEnumerable<IPerson> filterByRole(IDictionary<PersonFinderField, string> searchCriteria, IEnumerable<IPerson> people)
		{
			var roleName = searchCriteria[PersonFinderField.Role];
			roleName = Regex.Match(roleName, quotePattern).Value;
			people =
				people.Where(
					p => _personApplicationRoleDictionary.ContainsKey(p) && _personApplicationRoleDictionary[p] == roleName);
			return people;
		}

		private static IEnumerable<IPerson> filterByFirstName(IDictionary<PersonFinderField, string> searchCriteria, IEnumerable<IPerson> people)
		{
			var firstName = searchCriteria[PersonFinderField.FirstName];
			people = people.Where(p => p.Name.FirstName.IndexOf(firstName, StringComparison.OrdinalIgnoreCase) > -1);
			return people;
		}

	}
}
