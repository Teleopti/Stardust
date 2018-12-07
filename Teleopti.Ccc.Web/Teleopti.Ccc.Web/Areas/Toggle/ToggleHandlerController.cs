using System.Linq;
using System.Web.Http;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.Web.Core.Startup;

namespace Teleopti.Ccc.Web.Areas.Toggle
{
	public class ToggleHandlerController : ApiController
	{
		private readonly IToggleManager _toggleManager;
		private readonly ITogglesActive _togglesActive;

		public ToggleHandlerController(IToggleManager toggleManager, ITogglesActive togglesActive)
		{
			_toggleManager = toggleManager;
			_togglesActive = togglesActive;
		}

		/// <summary>
		/// Url
		/// <![CDATA[
		/// [web]/ToggleHandler/IsEnabled?toggle=[yourToggle]
		/// ]]>
		/// </summary>
		[HttpGet, Route("ToggleHandler/IsEnabled"), CacheFilterHttp]
		public IHttpActionResult IsEnabled(Toggles toggle)
		{
			return Ok(new ToggleEnabledResult
				{
					IsEnabled = _toggleManager.IsEnabled(toggle)
				});
		}

		/// <summary>
		/// Url
		/// <![CDATA[
		/// [web]/ToggleHandler/AllToggles
		/// ]]>
		/// </summary>
		[HttpGet, Route("ToggleHandler/AllToggles"), CacheFilterHttp]
		public IHttpActionResult AllToggles()
		{
			var dic = _togglesActive.AllActiveToggles()
				.ToDictionary(toggleInfo => toggleInfo.Key.ToString(), toggleInfo => toggleInfo.Value);

			return Ok(dic);
		}
	}
}
