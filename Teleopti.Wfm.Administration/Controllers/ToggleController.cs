using System.Web.Http;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.Toggle;

namespace Teleopti.Wfm.Administration.Controllers
{
	public class ToggleController : ApiController
	{
		private readonly IToggleManager _toggleManager;

		public ToggleController(IToggleManager toggleManager)
		{
			_toggleManager = toggleManager;
		}

		/// <summary>
		/// Url
		/// <![CDATA[
		/// [web]/Toggle/IsEnabled?toggle=[yourToggle]
		/// ]]>
		/// </summary>
		[HttpGet, Route("Toggle/IsEnabled")]
		public IHttpActionResult IsEnabled(Toggles toggle)
		{
			return Ok(new ToggleEnabledResult
			{
				IsEnabled = _toggleManager.IsEnabled(toggle)
			});
		}
	}
}