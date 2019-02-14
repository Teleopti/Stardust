using System.Web.Http;
using Teleopti.Ccc.Domain.MonitorSystem;

namespace Teleopti.Wfm.Administration.Monitoring
{
	public class MonitorController : ApiController
	{
		private readonly CheckLegacySystemStatus _checkLegacySystemStatus;

		public MonitorController(CheckLegacySystemStatus checkLegacySystemStatus)
		{
			_checkLegacySystemStatus = checkLegacySystemStatus;
		}
		
		[HttpGet]
		public IHttpActionResult Check()
		{
			_checkLegacySystemStatus.Execute();
			return Ok();
		}
	}
}