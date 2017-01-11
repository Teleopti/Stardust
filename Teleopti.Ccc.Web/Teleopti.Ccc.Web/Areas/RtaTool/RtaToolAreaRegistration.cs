using System.Web.Mvc;

namespace Teleopti.Ccc.Web.Areas.RtaTool
{
	public class RtaToolAreaRegistration : AreaRegistration
	{
		public override string AreaName => "RtaTool";

		public override void RegisterArea(AreaRegistrationContext context)
		{
			context.MapRoute(
				"OldRtaTool",
				"OldRtaTool/{controller}/{action}/{id}",
				new {controller = "Application", action = "Old", id = UrlParameter.Optional});

			context.MapRoute(
				"RtaTool_default",
				"RtaTool/{controller}/{action}/{id}",
				new { controller = "Application", action = "Index", id = UrlParameter.Optional }
			);
		}
	}
}
