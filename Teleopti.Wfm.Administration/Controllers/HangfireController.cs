using System.Web.Http;
using Teleopti.Wfm.Administration.Core;
using Teleopti.Wfm.Administration.Core.Hangfire;

namespace Teleopti.Wfm.Administration.Controllers
{
    [TenantTokenAuthentication]
    public class HangfireController : ApiController
	{
		private readonly HangfireStatisticsViewModelBuilder _statisticViewModelBuilder;

		public HangfireController(HangfireStatisticsViewModelBuilder statisticViewModelBuilder)
		{
			_statisticViewModelBuilder = statisticViewModelBuilder;
		}

		[HttpGet, Route("Hangfire/GetUrl")]
		public IHttpActionResult HangfireUrl()
		{
			// TODO Find better way of getting the real url so this works during development
			//return Ok("http://localhost:52858/hangfire");
			return Ok("hangfire");
		}
		
		[HttpGet, Route("Hangfire/Statistics2")]
		public IHttpActionResult HangfireStatistics2()
		{
			return Json(_statisticViewModelBuilder.BuildStatistics());
		}

	}
}
