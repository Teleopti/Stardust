using System.Web.Http;
using Teleopti.Wfm.Administration.Core;

namespace Teleopti.Wfm.Administration.Controllers
{
    [TenantTokenAuthentication]
    public class HangfireController : ApiController
	{
		[HttpGet, Route("Hangfire/GetUrl")]
		public IHttpActionResult HangfireUrl()
		{
			// TODO Find better way of getting the real url so this works during development
			//return Ok("http://localhost:52858/hangfire");
			return Ok("hangfire");
		}
	}
}
