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
			IOptionalColumnRepository optionalColumnRepository, IPersonAbsenceRepository personAbsenceRepository, ILoggedOnUser loggedOnUser)
		{
			_searchRepository = searchRepository;
			_personRepository = personRepository;
			_permissionProvider = permissionProvider;
			_optionalColumnRepository = optionalColumnRepository;
			_personAbsenceRepository = personAbsenceRepository;
			_loggedOnUser = loggedOnUser;
		}

		public PeopleSummaryModel SearchPermittedPeople(IDictionary<PersonFinderField, string> criteriaDictionary,
			int pageSize, int currentPageIndex, DateOnly currentDate, IDictionary<string, bool> sortedColumns, string function)
		{
			var optionalColumnCollection = _optionalColumnRepository.GetOptionalColumns<Person>();

			var personIdList = GetPermittedPersonIdList(criteriaDictionary,
				pageSize, currentPageIndex, currentDate, sortedColumns, function);
			var peopleList = _personRepository.FindPeople(personIdList).ToList();
			var totalPages = peopleList.Count == 0 ? 0 : peopleList.Count / pageSize + 1;

			return new PeopleSummaryModel
			{
				People = peopleList,
				TotalPages = totalPages,
				OptionalColumns = optionalColumnCollection
			};
		}

		public IEnumerable<IPerson> SearchPermittedPeople(IDictionary<PersonFinderField, string> criteriaDictionary, DateOnly dateInUserTimeZone, string function)
		{
			var personIdList = GetPermittedPersonIdList(criteriaDictionary, 9999, 1,dateInUserTimeZone, null, function);
			return _personRepository.FindPeople(personIdList).ToList();
		}

		public IEnumerable<IPerson> SearchPermittedPeopleWithAbsence(IDictionary<PersonFinderField, string> criteriaDictionary, DateOnly dateInUserTimeZone, string function)
		{
			var permittedPeople = SearchPermittedPeople(criteriaDictionary, dateInUserTimeZone, function);

			var startDateTime = new DateTime(dateInUserTimeZone.Year, dateInUserTimeZone.Month, dateInUserTimeZone.Day);
			startDateTime = TimeZoneHelper.ConvertToUtc(startDateTime, _loggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone());
			var endDateTime = new DateTime(dateInUserTimeZone.Year, dateInUserTimeZone.Month, dateInUserTimeZone.Day, 23, 59, 59);
			endDateTime = TimeZoneHelper.ConvertToUtc(endDateTime, _loggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone());
			var personAbsences = _personAbsenceRepository.Find(permittedPeople, new DateTimePeriod(startDateTime, endDateTime)).ToList();

			return personAbsences.Select(personAbsence => personAbsence.Person).ToList();
		}

		public IEnumerable<Guid> GetPermittedPersonIdList(IDictionary<PersonFinderField, string> criteriaDictionary,
			int pageSize, int currentPageIndex, DateOnly currentDate, IDictionary<string, bool> sortedColumns, string function)
		{
			var search = new PersonFinderSearchCriteria(criteriaDictionary, pageSize, currentDate,
				sortedColumns)
			{
				CurrentPage = currentPageIndex
			};
			_searchRepository.Find(search);

			var permittedPersonList =
				search.DisplayRows.Where(
					r =>
						r.RowNumber > 0 &&
						_permissionProvider.HasOrganisationDetailPermission(
						function,
							currentDate, r));
			return permittedPersonList.Select(x => x.PersonId);
		} 
	}
}