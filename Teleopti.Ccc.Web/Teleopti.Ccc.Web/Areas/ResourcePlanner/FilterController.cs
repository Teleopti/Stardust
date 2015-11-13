using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Optimization.Filters;

namespace Teleopti.Ccc.Web.Areas.ResourcePlanner
{
	public class FilterController : ApiController
	{
		private readonly FindFilter _findFilter;

		public FilterController(FindFilter findFilter)
		{
			_findFilter = findFilter;
		}

		[HttpGet, Route("api/filters/{searchString}/{maxHits}"), UnitOfWork]
		public virtual IHttpActionResult FindFilters(string searchString, int maxHits)
		{
			return Ok(_findFilter.Search(searchString, maxHits));
		}
  }
}