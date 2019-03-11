using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Teleopti.Ccc.Domain.Status;
using Teleopti.Ccc.Infrastructure.Status;

namespace Teleopti.Ccc.Web.Status
{
	public class StatusController : ApiController
	{
		private readonly ExecuteStatusStep _executeStatusStep;
		private readonly ListStatusSteps _listStatusSteps;
		private readonly PingCustomStep _pingCustomStep;

		public StatusController(ExecuteStatusStep executeStatusStep, 
			ListStatusSteps listStatusSteps,
			PingCustomStep pingCustomStep)
		{
			_executeStatusStep = executeStatusStep;
			_listStatusSteps = listStatusSteps;
			_pingCustomStep = pingCustomStep;
		}
		
		[HttpGet]
		[Route("status/check/{stepName}")]
		public HttpResponseMessage Check(string stepName)
		{
			var result = _executeStatusStep.Execute(stepName);
			return Request.CreateResponse(result.Success ? HttpStatusCode.OK : HttpStatusCode.InternalServerError, result.Output);
		}
		
		[HttpGet]
		[Route("status/list")]
		public IHttpActionResult List()
		{
			return Ok(_listStatusSteps.Execute(new Uri(Request.RequestUri, RequestContext.VirtualPathRoot), "status"));
		}

		[HttpGet]
		[Route("status/ping/{stepName}")]
		public IHttpActionResult Ping(string stepName)
		{
			var result = _pingCustomStep.Execute(stepName);
			return result ? (IHttpActionResult) Ok() : BadRequest();
		}
	}
}