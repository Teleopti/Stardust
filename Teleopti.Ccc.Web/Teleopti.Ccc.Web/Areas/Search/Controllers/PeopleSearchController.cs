using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Areas.People.Core.Providers;
using Teleopti.Ccc.Web.Filters;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Search.Controllers
{
	[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.WebPeople)]
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
			var peopleList = _searchProvider.SearchPeople(criteriaDictionary, pageSize, currentPageIndex, currentDate, sortColumns);

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
				Team = x.MyTeam(currentDate) == null ? "" : x.MyTeam(currentDate).SiteAndTeam
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
