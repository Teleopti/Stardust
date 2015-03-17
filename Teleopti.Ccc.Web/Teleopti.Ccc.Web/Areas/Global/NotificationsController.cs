using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;

namespace Teleopti.Ccc.Web.Areas.Global
{
	public class NotificationsController : ApiController
    {

		[UnitOfWork]
		[HttpGet, Route("api/notifications")]
		public virtual IHttpActionResult GetResult()
		{
			return null;
		}
	}
}