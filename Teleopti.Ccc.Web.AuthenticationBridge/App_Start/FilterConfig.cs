using System.Web;
using System.Web.Mvc;

namespace Teleopti.Ccc.Web.AuthenticationBridge
{
	public class FilterConfig
	{
		public static void RegisterGlobalFilters(GlobalFilterCollection filters)
		{
			filters.Add(new HandleErrorAttribute());
		}
	}
}