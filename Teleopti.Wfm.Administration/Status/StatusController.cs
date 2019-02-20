using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Teleopti.Ccc.Domain.Status;

namespace Teleopti.Wfm.Administration.Status
{
	public class StatusController : ApiController
	{
		private readonly ExecuteStatusStep _executeStatusStep;
		private readonly ListStatusSteps _listStatusSteps;
		private const string relPathToCheck = "status/check";

		public StatusController(ExecuteStatusStep executeStatusStep, ListStatusSteps listStatusSteps)
		{
			_executeStatusStep = executeStatusStep;
			_listStatusSteps = listStatusSteps;
		}
		
		[HttpGet]
		[Route(relPathToCheck + "/{statusStep}")]
		public HttpResponseMessage Check(string statusStep)
		{
			var result = _executeStatusStep.Execute(statusStep);
			return Request.CreateResponse(result.Success ? HttpStatusCode.OK : HttpStatusCode.InternalServerError, result.Output);
		}
		
		[HttpGet]
		[Route("status/list")]
		public IHttpActionResult List()
		{
			return Ok(_listStatusSteps.Execute(new Uri(Request.RequestUri, RequestContext.VirtualPathRoot) + "/" + relPathToCheck));
		}
	}
}