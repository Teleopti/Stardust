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

		public PeopleSearchController(IPeopleSearchProvider searchProvider)
		{
			_searchProvider = searchProvider;
		}

		[UnitOfWork]
		[HttpGet, Route("api/Search/People")]
		public virtual IHttpActionResult GetResult(string keyword, int pageSize, int currentPageIndex)
		{
			var currentDate = DateOnly.Today;
			var peopleList = _searchProvider.SearchPeople(keyword, pageSize, currentPageIndex, currentDate);
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
				CurrentPage = currentPageIndex,
				TotalPages = peopleList.TotalPages,
				OptionalColumns = peopleList.OptionalColumns.Select(x => x.Name)
			};
			return Ok(result);
		}
	}
}
