using System.Web.Http;

namespace Teleopti.Ccc.Web.Areas.Global
{
	public class StatusController : ApiController
	{
		private readonly IActionThrottler _actionThrottler;

		public StatusController(IActionThrottler actionThrottler)
		{
			_actionThrottler = actionThrottler;
		}

		[HttpGet, Route("api/Status/{actionName}"), Authorize]
		public dynamic Status(string actionName)
		{
			return new { IsRunning = _actionThrottler.IsBlocked(actionName) };
		}
	}
}