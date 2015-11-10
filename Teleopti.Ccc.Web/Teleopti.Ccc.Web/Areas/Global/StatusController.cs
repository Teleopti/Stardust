using System.Web.Http;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.Web.Areas.Global
{
	public class StatusController : ApiController
	{
		private readonly IActionThrottler _actionThrottler;

		public StatusController(IActionThrottler actionThrottler)
		{
			_actionThrottler = actionThrottler;
		}

		[HttpGet, Route("api/Status/{actionName}"), AuthorizeTeleopti]
		public dynamic Status(string actionName)
		{
			return new { IsRunning = _actionThrottler.IsBlocked(actionName) };
		}
	}
}