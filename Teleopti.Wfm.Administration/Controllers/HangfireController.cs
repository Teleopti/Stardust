using System.Web.Http;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Wfm.Administration.Core;
using Teleopti.Wfm.Administration.Core.Hangfire;

namespace Teleopti.Wfm.Administration.Controllers
{
    [TenantTokenAuthentication]
    public class HangfireController : ApiController
	{
		private readonly HangfireStatisticsViewModelBuilder _statisticViewModelBuilder;
		private readonly IManageFailedHangfireEvents _manageFailedHangfireEvents;

		public HangfireController(HangfireStatisticsViewModelBuilder statisticViewModelBuilder, IManageFailedHangfireEvents manageFailedHangfireEvents)
		{
			_statisticViewModelBuilder = statisticViewModelBuilder;
			_manageFailedHangfireEvents = manageFailedHangfireEvents;
		}

		[HttpGet, Route("Hangfire/GetUrl")]
		public IHttpActionResult HangfireUrl()
		{
			// TODO Find better way of getting the real url so this works during development
			//return Ok("http://localhost:52858/hangfire");
			return Ok("hangfire");
		}

		[HttpGet, Route("Hangfire/Statistics")]
		public IHttpActionResult HangfireStatistics()
		{
			return Json(_statisticViewModelBuilder.Build());
		}

		[HttpGet, Route("Hangfire/TypesOfSucceededEvents")]
		public IHttpActionResult HangfireTypesOfSucceededEvents()
		{
			return Json(_statisticViewModelBuilder.BuildTypesOfEvents("Succeeded"));
		}

		[HttpGet, Route("Hangfire/TypesOfFailedEvents")]
		public IHttpActionResult HangfireTypesOfFailedEvents()
		{
			return Json(_statisticViewModelBuilder.BuildTypesOfEvents("Failed"));
		}

		[HttpPost, Route("Hangfire/RequeueFailed")]
		public IHttpActionResult RequeueFailed([FromBody]string eventName)
		{
			_manageFailedHangfireEvents.RequeueFailed(eventName, null, null);
			return Ok();
		}

		[HttpPost, Route("Hangfire/DeleteFailed")]
		public IHttpActionResult DeleteFailed([FromBody]string eventName)
		{
			_manageFailedHangfireEvents.DeleteFailed(eventName, null, null);
			return Ok();
		}
	}
}
