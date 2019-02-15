using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;
using Teleopti.Ccc.Domain.MonitorSystem;

namespace Teleopti.Wfm.Administration.Monitoring
{
	public class MonitorController : ApiController
	{
		private readonly TryExecuteMonitorStep _tryExecuteMonitorStep;
		private readonly ListMonitorSteps _listMonitorSteps;

		public MonitorController(TryExecuteMonitorStep tryExecuteMonitorStep, ListMonitorSteps listMonitorSteps)
		{
			_tryExecuteMonitorStep = tryExecuteMonitorStep;
			_listMonitorSteps = listMonitorSteps;
		}
		
		[HttpGet]
		[Route("monitor/check/{monitorStep}")]
		public HttpResponseMessage Check(string monitorStep)
		{
			var result = _tryExecuteMonitorStep.TryExecute(monitorStep);
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