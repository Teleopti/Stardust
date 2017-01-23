using System.Web.Mvc;

namespace Teleopti.Ccc.Web.Areas.Start
{
	public class StartAreaRegistration : AreaRegistration
	{
		public override string AreaName => "Start";

		public override void RegisterArea(AreaRegistrationContext context)
		{
			context.MapRoute(
				"Start-default",
				"Start/{controller}/{action}/{id}",
				new { controller = "Authentication", action = "SignIn", id = UrlParameter.Optional }
				);
		}
	}
}
