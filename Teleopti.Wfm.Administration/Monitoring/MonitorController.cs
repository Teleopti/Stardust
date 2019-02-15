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
		public IHttpActionResult Check(string monitorStep)
		{
			if (_tryExecuteMonitorStep.TryExecute(monitorStep, out var result))
				return Ok(result);
			return BadRequest($"{monitorStep} is not a known monitor step");
		}
		
		[HttpGet]
		[Route("monitor/list")]
		public IHttpActionResult List()
		{
			return Ok(_listMonitorSteps.Execute());
		}
	}
}