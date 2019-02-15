using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;
using Teleopti.Ccc.Domain.MonitorSystem;

namespace Teleopti.Wfm.Administration.Monitoring
{
	public class MonitorController : ApiController
	{
		private readonly ExecuteMonitorStep _executeMonitorStep;
		private readonly ListMonitorSteps _listMonitorSteps;

		public MonitorController(ExecuteMonitorStep executeMonitorStep, ListMonitorSteps listMonitorSteps)
		{
			_executeMonitorStep = executeMonitorStep;
			_listMonitorSteps = listMonitorSteps;
		}
		
		[HttpGet]
		[Route("monitor/check/{monitorStep}")]
		public HttpResponseMessage Check(string monitorStep)
		{
			var result = _executeMonitorStep.Execute(monitorStep);
			return Request.CreateResponse(result.Success ? HttpStatusCode.OK : HttpStatusCode.InternalServerError, result.Output);
		}
		
		[HttpGet]
		[Route("monitor/list")]
		public IHttpActionResult List()
		{
			return Ok(_listMonitorSteps.Execute());
		}
	}
}