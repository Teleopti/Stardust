using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;

namespace Teleopti.Ccc.Web.Areas.ResourcePlanner
{
	public class OptimizationController : ApiController
    {
		[HttpPost, Route("api/ResourcePlanner/optimize/FixedStaff"), Authorize,
		 UnitOfWork]
		public virtual IHttpActionResult FixedStaff([FromBody] FixedStaffSchedulingInput input)
		{
			return
				Ok("Optimization was called");
		}
    }
}