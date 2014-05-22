using System.Web.Mvc;
using Teleopti.Ccc.Domain.FeatureFlags;

namespace Teleopti.Ccc.Web.Areas.Toggle
{
	public class ToggleHandlerController : Controller
	{
		private readonly IToggleManager _toggleManager;

		public ToggleHandlerController(IToggleManager toggleManager)
		{
			_toggleManager = toggleManager;
		}

		public bool IsEnabled(Toggles toggle)
		{
			return _toggleManager.IsEnabled(toggle);
		}
	}
}
