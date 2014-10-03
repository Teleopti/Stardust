using System.Web.Mvc;

namespace Teleopti.Ccc.Web.Areas.RtaTool
{
	public class RtaToolAreaRegistration : AreaRegistration
	{
		public override string AreaName
		{
			get
			{
				return "RtaTool";
			}
		}

		public override void RegisterArea(AreaRegistrationContext context)
		{
			context.MapRoute(
				"RtaTool_default",
				"RtaTool/{controller}/{action}/{id}",
				new { controller = "Application" ,action = "Index", id = UrlParameter.Optional }
			);
		}
	}
}
