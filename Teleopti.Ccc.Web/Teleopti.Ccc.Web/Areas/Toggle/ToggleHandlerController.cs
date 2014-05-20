using System.Web.Mvc;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.Web.Areas.Toggle
{
	public class ToggleHandlerController : Controller
	{
		private readonly IToggleManager _toggleManager;

		public ToggleHandlerController(IToggleManager toggleManager)
		{
			_toggleManager = toggleManager;
		}

		[UnitOfWorkAction] //remove when an interface returning personid exist
		public bool IsEnabled(Toggles toggle)
		{
			return _toggleManager.IsEnabled(toggle);
		}
	}
}
