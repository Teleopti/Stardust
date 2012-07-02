using System.Linq;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Portal;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Portal
{
	public class PortalHtmlHelper
	{
		public string GetDefaultAction(PortalViewModel portalViewModel)
		{
			return GetAction(portalViewModel.NavigationItems.First());
		}

		public string GetAction(NavigationItem navItem)
		{
			return string.Format("{0}/{1}", navItem.Controller, navItem.Action);
		}

		public string GetId(NavigationItem navItem)
		{
			return string.Format("{0}_{1}", navItem.Controller, navItem.Action);
		}
	}
}