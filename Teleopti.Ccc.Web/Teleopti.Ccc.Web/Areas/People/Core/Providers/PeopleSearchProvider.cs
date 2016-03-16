using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
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
		private readonly IPersonAbsenceRepository _personAbsenceRepository;
		private readonly ILoggedOnUser _loggedOnUser;

		public PeopleSearchProvider(
			IPersonFinderReadOnlyRepository searchRepository,
			IPersonRepository personRepository,
			IPermissionProvider permissionProvider,
			IOptionalColumnRepository optionalColumnRepository, IPersonAbsenceRepository personAbsenceRepository,
			ILoggedOnUser loggedOnUser)
		{
			_searchRepository = searchRepository;
			_personRepository = personRepository;
			_permissionProvider = permissionProvider;
			_optionalColumnRepository = optionalColumnRepository;
			_personAbsenceRepository = personAbsenceRepository;
			_loggedOnUser = loggedOnUser;
		}

		public PeopleSummaryModel SearchPermittedPeopleSummary(IDictionary<PersonFinderField, string> criteriaDictionary,
			int pageSize, int currentPageIndex, DateOnly currentDate, IDictionary<string, bool> sortedColumns, string function)
		{
			var personIdList = getPermittedPersonIdList(criteriaDictionary,pageSize, currentPageIndex, currentDate,
				sortedColumns, function);

			return searchPermittedPeopleSummary(personIdList, pageSize);
		}

		public IEnumerable<IPerson> SearchPermittedPeople(IDictionary<PersonFinderField, string> criteriaDictionary,
			DateOnly dateInUserTimeZone, string function)
		{
			var searchCriteria = CreatePersonFinderSearchCriteria(criteriaDictionary, 9999, 1, dateInUserTimeZone,
				null);
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
			var startDateTime = dateInUserTimeZone.Date;
			startDateTime = TimeZoneHelper.ConvertToUtc(startDateTime,
				_loggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone());

			var endDateTime = dateInUserTimeZone.Date.AddDays(1).AddSeconds(-1);
			endDateTime = TimeZoneHelper.ConvertToUtc(endDateTime,
				_loggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone());

			var personAbsences =
				_personAbsenceRepository.Find(permittedPeople, new DateTimePeriod(startDateTime, endDateTime)).ToList();

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

		public PersonFinderSearchCriteria CreatePersonFinderSearchCriteria(IDictionary<PersonFinderField, string> criteriaDictionary,
			int pageSize, int currentPageIndex, DateOnly currentDate, IDictionary<string, bool> sortedColumns)
		{
			var search = new PersonFinderSearchCriteria(criteriaDictionary, pageSize, currentDate, sortedColumns)
			{
				CurrentPage = currentPageIndex
			};
			_searchRepository.Find(search);
			return search;
		}

		private IEnumerable<Guid> getPermittedPersonIdList(IDictionary<PersonFinderField, string> criteriaDictionary,
			int pageSize, int currentPageIndex, DateOnly currentDate, IDictionary<string, bool> sortedColumns, string function)
		{
			var search = CreatePersonFinderSearchCriteria(criteriaDictionary, pageSize, currentPageIndex, currentDate, sortedColumns);

			var permittedPersonList =
				search.DisplayRows.Where(
					r => r.RowNumber > 0 && _permissionProvider.HasOrganisationDetailPermission(function, currentDate, r));
			return permittedPersonList.Select(x => x.PersonId);
		}

		private PeopleSummaryModel searchPermittedPeopleSummary(IEnumerable<Guid> personIds, int pageSize)
		{
			var optionalColumnCollection = _optionalColumnRepository.GetOptionalColumns<Person>();
			var peopleList = _personRepository.FindPeople(personIds).ToList();
			var totalPages = peopleList.Count == 0 ? 0 : peopleList.Count / pageSize + 1;

			return new PeopleSummaryModel
			{
				People = peopleList,
				TotalPages = totalPages,
				OptionalColumns = optionalColumnCollection
			};
		}
	}
}
