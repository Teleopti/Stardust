using System;
using System.Web.Routing;
using AuthBridge.Web;

namespace Teleopti.Ccc.Web.AuthenticationBridge
{
	public class Global : MvcApplication
	{
		private void Application_Start(object sender, EventArgs e)
		{
			registerNewRoutes(RouteTable.Routes);
		}
		
		private static void registerNewRoutes(RouteCollection routes)
		{
			routes.MapPageRoute("","SystemStatus", "~/SystemStatus.aspx");
		}
	}
}