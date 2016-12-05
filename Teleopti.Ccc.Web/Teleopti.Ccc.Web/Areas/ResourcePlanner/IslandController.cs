using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Islands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.ResourcePlanner
{
	public class IslandController : ApiController
	{
		private readonly ICreateIslands _createIslands;

		public IslandController(ICreateIslands createIslands)
		{
			_createIslands = createIslands;
		}

		[UnitOfWork]
		[HttpGet, Route("api/ResourcePlanner/Islands")]
		public virtual IHttpActionResult Islands()
		{
			return Json(_createIslands.Create(DateOnly.Today.ToDateOnlyPeriod()));
		}
	}
}