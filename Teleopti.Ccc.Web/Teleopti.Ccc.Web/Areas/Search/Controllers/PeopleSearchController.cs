using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
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
			var criteriaDictionary = new Dictionary<PersonFinderField, string>();
			var myTeam = _loggonUser.CurrentUser().MyTeam(currentDate);

			if (string.IsNullOrEmpty(keyword) && myTeam == null)
			{
				return Ok(new {});
			}

			if (string.IsNullOrEmpty(keyword))
			{
				keyword = myTeam.Description.Name;
				criteriaDictionary.Add(PersonFinderField.Organization, keyword);
			}
			else
			{
				criteriaDictionary.Add(PersonFinderField.All, keyword);
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
	}
}
