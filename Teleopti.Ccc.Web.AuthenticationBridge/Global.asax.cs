using System.Web.Routing;
using AuthBridge.Web;

namespace Teleopti.Ccc.Web.AuthenticationBridge
{
	public class Global : MvcApplication
	{
		protected new void Application_Start()
		{
			base.Application_Start();
			
			RouteTable.Routes.MapPageRoute("", "SystemStatus", "~/SystemStatus.aspx");
			RegisterRoutes(RouteTable.Routes);
		}
	}
}