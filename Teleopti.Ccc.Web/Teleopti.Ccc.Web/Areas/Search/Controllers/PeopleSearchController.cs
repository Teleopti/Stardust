using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Newtonsoft.Json;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Web.Areas.People.Core.Providers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Search.Controllers
{
	public class PeopleSearchController : ApiController
	{
		private readonly IPeopleSearchProvider _searchProvider;
		private readonly ILoggedOnUser _loggonUser;

		public PeopleSearchController(IPeopleSearchProvider searchProvider, ILoggedOnUser loggonUser)
		{
			_searchProvider = searchProvider;
			_loggonUser = loggonUser;
		}

		[UnitOfWork]
		[HttpGet, Route("api/Search/People/Keyword")]
		public virtual IHttpActionResult GetResult(string keyword, int pageSize, int currentPageIndex)
		{
			var currentDate = DateOnly.Today;
			IDictionary<PersonFinderField, string> criteriaDictionary = null;
			var myTeam = _loggonUser.CurrentUser().MyTeam(currentDate);

			if (string.IsNullOrEmpty(keyword) && myTeam == null)
			{
				return Ok(new {});
			}
			;
			if (string.IsNullOrEmpty(keyword))
			{
				keyword = myTeam.Description.Name;
				criteriaDictionary = new Dictionary<PersonFinderField, string>{{PersonFinderField.Organization, keyword}};
			}
			else
			{
				criteriaDictionary = SearchTermParser.Parse(keyword);
			}

			var peopleList = _searchProvider.SearchPeople(criteriaDictionary, pageSize, currentPageIndex, currentDate);
			var resultPeople = peopleList.People.Select(x => new
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
				Team = x.MyTeam(currentDate) == null ? "" : x.MyTeam(currentDate).SiteAndTeam
			}).OrderBy(p => p.LastName);

			var result = new
			{
				People = resultPeople,
				CurrentPage = currentPageIndex,
				TotalPages = peopleList.TotalPages,
				OptionalColumns = peopleList.OptionalColumns.Select(x => x.Name)
			};
			return Ok(result);
		}

		[UnitOfWork]
		[HttpPost, Route("api/Search/People/Criteria")]
		public virtual IHttpActionResult GetAdvancedSearchResult(AdvancedSearchInput input)
		{
			var currentDate = DateOnly.Today;
			var criteriaDictionary = new Dictionary<PersonFinderField, string>();
			if (!string.IsNullOrEmpty(input.SearchCriteria.FirstName))
			{
				criteriaDictionary.Add(PersonFinderField.FirstName, input.SearchCriteria.FirstName);
			}
			if (!string.IsNullOrEmpty(input.SearchCriteria.LastName))
			{
				criteriaDictionary.Add(PersonFinderField.LastName, input.SearchCriteria.LastName);
			}
			if (!string.IsNullOrEmpty(input.SearchCriteria.BudgetGroup))
			{
				criteriaDictionary.Add(PersonFinderField.BudgetGroup, input.SearchCriteria.BudgetGroup);
			}
			if (!string.IsNullOrEmpty(input.SearchCriteria.Contract))
			{
				criteriaDictionary.Add(PersonFinderField.Contract, input.SearchCriteria.Contract);
			}
			if (!string.IsNullOrEmpty(input.SearchCriteria.ContractSchedule))
			{
				criteriaDictionary.Add(PersonFinderField.ContractSchedule, input.SearchCriteria.ContractSchedule);
			}
			if (!string.IsNullOrEmpty(input.SearchCriteria.EmploymentNumber))
			{
				criteriaDictionary.Add(PersonFinderField.EmploymentNumber, input.SearchCriteria.EmploymentNumber);
			}
			if (!string.IsNullOrEmpty(input.SearchCriteria.Note))
			{
				criteriaDictionary.Add(PersonFinderField.Note, input.SearchCriteria.Note);
			}
			if (!string.IsNullOrEmpty(input.SearchCriteria.Organization))
			{
				criteriaDictionary.Add(PersonFinderField.Organization, input.SearchCriteria.Organization);
			}
			if (!string.IsNullOrEmpty(input.SearchCriteria.PartTimePercentage))
			{
				criteriaDictionary.Add(PersonFinderField.PartTimePercentage, input.SearchCriteria.PartTimePercentage);
			}
			if (!string.IsNullOrEmpty(input.SearchCriteria.Role))
			{
				criteriaDictionary.Add(PersonFinderField.Role, input.SearchCriteria.Role);
			}
			if (!string.IsNullOrEmpty(input.SearchCriteria.ShiftBag))
			{
				criteriaDictionary.Add(PersonFinderField.ShiftBag, input.SearchCriteria.ShiftBag);
			}
			if (!string.IsNullOrEmpty(input.SearchCriteria.Skill))
			{
				criteriaDictionary.Add(PersonFinderField.Skill, input.SearchCriteria.Skill);
			}
			var peopleList = _searchProvider.SearchPeople(criteriaDictionary, input.PageSize, input.CurrentPageIndex, currentDate);
			var resultPeople = peopleList.People.Select(x => new
			{
				FirstName = x.Name.FirstName,
				LastName = x.Name.LastName,
				EmploymentNumber = x.EmploymentNumber,
				PersonId = x.Id.Value,
				Email = x.Email,
				LeavingDate = x.TerminalDate == null ? "" : x.TerminalDate.Value.ToShortDateString(),
				OptionalColumnValues = peopleList.OptionalColumns.Select(c =>
				{
					var columnValue = x.GetColumnValue(c);
					var value = columnValue == null ? "" : columnValue.Description;
					return new KeyValuePair<string, string>(c.Name, value);
				}),
				Team = x.MyTeam(currentDate) == null ? "" : x.MyTeam(currentDate).SiteAndTeam
			}).OrderBy(p => p.LastName);

			var result = new
			{
				People = resultPeople,
				CurrentPage = input.CurrentPageIndex,
				TotalPages = peopleList.TotalPages,
				OptionalColumns = peopleList.OptionalColumns.Select(x => x.Name)
			};
			return Ok(result);
		}
	}

	public class AdvancedSearchInput
	{
		public PeopleSearchCriteria SearchCriteria { get; set; }
		public int PageSize { get; set; }
		public int CurrentPageIndex { get; set; }
	}

	public class PeopleSearchCriteria
	{
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string EmploymentNumber { get; set; }
		public string BudgetGroup { get; set; }
		public string Contract { get; set; }
		public string ContractSchedule { get; set; }
		public string Note { get; set; }
		public string Organization { get; set; }
		public string PartTimePercentage { get; set; }
		public string Role { get; set; }
		public string ShiftBag { get; set; }
		public string Skill { get; set; }
	}
}
