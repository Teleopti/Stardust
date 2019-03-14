using System;
using System.Web.Http;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.Infrastructure.Toggle.Admin;

namespace Teleopti.Wfm.Administration.Controllers
{
	public class ToggleController : ApiController
	{
		private readonly IToggleManager _toggleManager;

		public ToggleController(IToggleManager toggleManager)
		{
			_toggleManager = toggleManager;
		}

		[HttpGet, Route("Toggle/IsEnabled")]
		public IHttpActionResult IsEnabled(string toggle)
		{
			Toggles enumToggle = (Toggles)Enum.Parse(typeof(Toggles), toggle, true);
			return Json(_toggleManager.IsEnabled(enumToggle));
		}
	}
}