using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.WebPages;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.PeopleSearch;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Web.Areas.Search.Models;
using Teleopti.Ccc.Web.Filters;


namespace Teleopti.Ccc.Web.Areas.Search.Controllers
{
	[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.PeopleAccess)]
	public class PeopleSearchController : ApiController
	{
		private readonly IPersonRepository _personRepository;
		private readonly IPeopleSearchProvider _searchProvider;
		private readonly ILoggedOnUser _loggonUser;
		private readonly IPersonFinderReadOnlyRepository _personFinder;
		private readonly IAuthorization _auth;

		public PeopleSearchController(
			IPeopleSearchProvider searchProvider,
			ILoggedOnUser loggonUser,
			IPersonFinderReadOnlyRepository personFinder,
			IPersonRepository personRepository,
			IAuthorization auth)
		{
			_searchProvider = searchProvider;
			_loggonUser = loggonUser;
			_personFinder = personFinder;
			_personRepository = personRepository;
			_auth = auth;
		}

		[UnitOfWork]
		[HttpPost, Route("api/Search/FindPeople")]
		public virtual FindPeopleViewModel FindPeople(FindPeopleInputModel searchCritera)
		{
			searchCritera.SearchDate = DateTime.Now;
			var canSeeUsers = _auth.IsPermitted(DefinedRaptorApplicationFunctionPaths.PeopleManageUsers, DateOnly.Today, _loggonUser.CurrentUser());
			var currentBusinessUnit = ServiceLocatorForEntity.CurrentBusinessUnit.Current().Id.GetValueOrDefault();

			var personFinderSearchCriteria = new PeoplePersonFinderSearchWithPermissionCriteria(
				PersonFinderField.All, 
				searchCritera.KeyWord, 
				searchCritera.PageIndex + 1, 
				searchCritera.PageSize, 
				DateOnly.Today, 
				searchCritera.SortColumn, 
				searchCritera.Direction,
				searchCritera.SearchDate.ToDateOnly(), 
				_loggonUser.CurrentUser().Id.GetValueOrDefault(), 
				DefinedRaptorApplicationFunctionForeignIds.PeopleAccess, 
				canSeeUsers, 
				currentBusinessUnit);

			_personFinder.FindPeopleWithDataPermission(personFinderSearchCriteria);

			var findPeopleViewModel = new FindPeopleViewModel();
			var persons = _personRepository.FindPeople(personFinderSearchCriteria.DisplayRows.Select(x => x.PersonId));

			foreach (var personFinderDisplayRow in personFinderSearchCriteria.DisplayRows)
			{
				var person = persons.FirstOrDefault(x => x.Id == personFinderDisplayRow.PersonId);
				var personPeriod = person.Period(searchCritera.SearchDate.ToDateOnly());
				var team = personPeriod?.Team?.Description.Name ?? string.Empty;
				var site = personPeriod?.Team?.Site?.Description.Name ?? string.Empty;
				var pers = new PeopleViewModel
				{
					FirstName = person.Name.FirstName,
					LastName = person.Name.LastName,
					Id = person.Id.GetValueOrDefault().ToString(),
					SiteTeam = string.Concat(site, team.IsEmpty() ? "" : "/", team),
					Team = team,
					Site = site,
					Roles = person.PermissionInformation.ApplicationRoleCollection.Select(x => new SearchPersonRoleViewModel
					{
						Id = x.Id.GetValueOrDefault(),
						Name = x.DescriptionText,
					}).ToList()
				};
				findPeopleViewModel.People.Add(pers);
			}

			findPeopleViewModel.TotalRows = personFinderSearchCriteria.TotalRows;
			findPeopleViewModel.PageIndex = searchCritera.PageIndex;

			return findPeopleViewModel;
		}

		[UnitOfWork]
		[HttpGet, Route("api/Search/People/Keyword")]
		public virtual IHttpActionResult GetResult(string keyword, int pageSize, int currentPageIndex, string sortColumns)
		{
			var currentDate = DateOnly.Today;
			var myTeam = _loggonUser.CurrentUser().MyTeam(currentDate);

			if (string.IsNullOrEmpty(keyword) && myTeam == null)
			{
				return Ok(new {});
			}

			if (string.IsNullOrEmpty(keyword))
			{
				var siteTerm = myTeam.Site.Description.Name.Contains(" ")
					? "\"" + myTeam.Site.Description.Name + "\""
					: myTeam.Site.Description.Name;
				var teamTerm = myTeam.Description.Name.Contains(" ")
					? "\"" + myTeam.Description.Name + "\""
					: myTeam.Description.Name;
				keyword = siteTerm + " " + teamTerm;
			}

			// Key is column name for sort, value is sort by ascending or not
			var sortColumnList = new Dictionary<string, bool>();
			if (string.IsNullOrEmpty(sortColumns))
			{
				sortColumnList.Add("lastname", true);
			}
			else
			{
				var columns = sortColumns.Trim().Split(';');
				foreach (var col in columns)
				{
					var parts = col.Split(':');
					if (parts.Length == 2)
					{
						sortColumnList.Add(parts[0].Trim(), bool.Parse(parts[1].Trim()));
					}
				}
			}

			var criteriaDictionary = SearchTermParser.Parse(keyword);
			var result = constructResult(pageSize, currentPageIndex, sortColumnList, criteriaDictionary, currentDate);
			return Ok(result);
		}
		
		private object constructResult(int pageSize, int currentPageIndex, IDictionary<string, bool> sortColumns, IDictionary<PersonFinderField, 
			string> criteriaDictionary, DateOnly currentDate)
		{
			var peopleList = _searchProvider.SearchPermittedPeopleSummary(criteriaDictionary, pageSize, currentPageIndex, currentDate, sortColumns, DefinedRaptorApplicationFunctionPaths.PeopleAccess);

			const string dateFormat = "yyyy-MM-dd";
			// This list should keep same as in SP "ReadModel.PersonFinderWithCriteria"
			var columnAndSortFuncMapping = new Dictionary<string, Func<IPerson, string>>
			{
				{"firstname", x => x.Name.FirstName},
				{"lastname", x => x.Name.LastName},
				{"employmentnumber", x => x.EmploymentNumber},
				{"note", x => x.Note},
				{
					"terminaldate", x => x.TerminalDate.HasValue
						? x.TerminalDate.Value.Date.ToString(dateFormat)
						: DateTime.MaxValue.ToString(dateFormat)
				}
			};

			if (sortColumns == null || !sortColumns.Any())
			{
				sortColumns = new Dictionary<string, bool> {{"lastname", true}};
			}
			var firstSortColumn = sortColumns.First();
			var columnName = firstSortColumn.Key.Trim().ToLower();
			var sortFunc = columnAndSortFuncMapping[columnName];
			var orderedPerson = firstSortColumn.Value
				? peopleList.People.OrderBy(sortFunc)
				: peopleList.People.OrderByDescending(sortFunc);

			var index = 0;
			foreach (var sortCol in sortColumns)
			{
				if (index == 0)
				{
					index++;

					continue;
				}

				columnName = sortCol.Key.Trim().ToLower();
				if (columnAndSortFuncMapping.ContainsKey(columnName))
				{
					sortFunc = columnAndSortFuncMapping[columnName];
					orderedPerson = sortCol.Value
						? orderedPerson.ThenBy(sortFunc)
						: orderedPerson.ThenByDescending(sortFunc);
				}

			}

			var resultPeople = orderedPerson.Select(x => new
			{
				FirstName = x.Name.FirstName,
				LastName = x.Name.LastName,
				EmploymentNumber = x.EmploymentNumber,
				PersonId = x.Id.GetValueOrDefault(),
				Email = x.Email,
				LeavingDate = x.TerminalDate == null ? "" : x.TerminalDate.Value.ToShortDateString(),
				OptionalColumnValues = peopleList.OptionalColumns.Select(c =>
				{
					var columnValue = x.GetColumnValue(c);
					var value = columnValue == null ? "" : columnValue.Description;
					return new KeyValuePair<string, string>(c.Name, value);
				}),
				Team = x.MyTeam(currentDate)?.SiteAndTeam ?? ""
			});

			var result = new
			{
				People = resultPeople,
				CurrentPage = currentPageIndex,
				TotalPages = peopleList.TotalPages,
				OptionalColumns = peopleList.OptionalColumns.Select(x => x.Name)
			};
			return result;
		}
	}
}
