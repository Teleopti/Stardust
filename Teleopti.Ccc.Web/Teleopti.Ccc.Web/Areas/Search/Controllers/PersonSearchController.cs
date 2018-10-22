using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Search;
using Teleopti.Ccc.Web.Areas.Search.Models;

namespace Teleopti.Ccc.Web.Areas.Search.Controllers
{
	public class PersonSearchController : ApiController
	{
		private readonly PersonSearchProvider _personSearchProvider;

		public PersonSearchController(PersonSearchProvider personSearchProvider)
		{
			_personSearchProvider = personSearchProvider;
		}

		[UnitOfWork]
		[HttpGet, Route("api/Search/FindPersonsByKeywords")]
		public virtual IHttpActionResult FindPersonsByKeywords(string keywords)
		{
			var persons = _personSearchProvider.FindPersonsByKeywords(keywords);
			return Ok(new FindPersonViewModel(persons));
		}
	}
}