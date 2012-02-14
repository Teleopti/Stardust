using System.Web.Mvc;

namespace Teleopti.Ccc.Web.Areas.Start
{
	public class StartAreaRegistration : AreaRegistration
	{
		public override string AreaName
		{
			get
			{
				return "Start";
			}
		}

		public override void RegisterArea(AreaRegistrationContext context)
		{
			context.MapRoute(
				"Start-default",
				"Start/{controller}/{action}/{id}",
				new { controller = "Menu", action = "Index", id = UrlParameter.Optional }
				);
		}
	}
}
