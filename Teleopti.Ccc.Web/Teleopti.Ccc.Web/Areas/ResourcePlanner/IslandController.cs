using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Islands.ClientModel;

namespace Teleopti.Ccc.Web.Areas.ResourcePlanner
{
	public class IslandController : ApiController
	{
		private readonly IslandModelFactory _islandModelFactory;

		public IslandController(IslandModelFactory islandModelFactory)
		{
			_islandModelFactory = islandModelFactory;
		}

		[UnitOfWork]
		[HttpGet, Route("api/ResourcePlanner/Islands")]
		public virtual IHttpActionResult Islands()
		{
			return Json(_islandModelFactory.Create());
		}
	}
}