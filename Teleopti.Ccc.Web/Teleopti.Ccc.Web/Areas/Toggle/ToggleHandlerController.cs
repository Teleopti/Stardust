using System.Linq;
using System.Web.Mvc;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Toggle;

namespace Teleopti.Ccc.Web.Areas.Toggle
{
	public class ToggleHandlerController : Controller
	{
		private readonly IToggleManager _toggleManager;
		private readonly ITogglesActive _togglesActive;

		public ToggleHandlerController(IToggleManager toggleManager, ITogglesActive togglesActive)
		{
			_toggleManager = toggleManager;
			_togglesActive = togglesActive;
		}

		/// <summary>
		/// Url if not logged in (from desktop app)
		/// <![CDATA[
		/// [web]/ToggleHandler/IsEnabled?toggle=[yourToggle]&datasource=[your datasource]
		/// ]]>
		/// Url if logged in
		/// <![CDATA[
		/// [web]/ToggleHandler/IsEnabled?toggle=[yourToggle]
		/// ]]>
		/// </summary>
		[HttpGet]
		public JsonResult IsEnabled(Toggles toggle)
		{
			return Json(new ToggleEnabledResult
				{
					IsEnabled = _toggleManager.IsEnabled(toggle)
				},
				JsonRequestBehavior.AllowGet
			);
		}

		/// <summary>
		/// Url if not logged in
		/// <![CDATA[
		/// [web]/ToggleHandler/AllToggles?datasource=[your datasource]
		/// ]]>
		/// Url if logged in
		/// <![CDATA[
		/// [web]/ToggleHandler/AllToggles
		/// ]]>
		/// </summary>
		[HttpGet]
		public JsonResult AllToggles()
		{
			var dic = _togglesActive.AllActiveToggles()
				.ToDictionary(toggleInfo => toggleInfo.Key.ToString(), toggleInfo => toggleInfo.Value);

			return Json(dic, JsonRequestBehavior.AllowGet);
		}
	}
}
