using System.Linq;
using System.Web.Mvc;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.Foundation;

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
		/// Url
		/// <![CDATA[
		/// [web]/ToggleHandler/IsEnabled?toggle=[yourToggle]&datasource=[your datasource]
		/// ]]>
		/// </summary>
		public bool IsEnabled(Toggles toggle)
		{
			return _toggleManager.IsEnabled(toggle);
		}

		/// <summary>
		/// Not used in code. Debug usage only!
		/// Url
		/// <![CDATA[
		/// [web]/ToggleHandler/IsEnabled?toggle=[yourToggle]&datasource=[your datasource]
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
