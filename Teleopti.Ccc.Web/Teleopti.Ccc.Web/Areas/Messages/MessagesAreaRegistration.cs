using System.Web.Mvc;

namespace Teleopti.Ccc.Web.Areas.Messages
{
	public class MessagesAreaRegistration : AreaRegistration 
	{
		public override string AreaName 
		{
			get 
			{
				return "Messages";
			}
		}

		public override void RegisterArea(AreaRegistrationContext context) 
		{
			var mapRoute = context.MapRoute(
				"Messages-authentication",
				"Messages/Authentication/{action}",
				new { controller = "Authentication", action = "SignIn", area = "Start", origin = "Messages" },
				null,
				new[] { "Teleopti.Ccc.Web.Areas.Start.*" });
			mapRoute.DataTokens["area"] = "Start";

			context.MapRoute(
				"Messages_default",
				"Messages/{controller}/{action}/{id}",
				new { controller = "Application", action = "Index", id = UrlParameter.Optional }
			);
		}
	}
}