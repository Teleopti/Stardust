using System.Web.Mvc;

namespace Teleopti.Ccc.Web.Areas.Insights
{
	public class InsightsAreaRegistration : AreaRegistration 
	{
		public override string AreaName => "Insights";

		public override void RegisterArea(AreaRegistrationContext context) 
		{
			context.MapRoute(
				"Insights_default",
				"Insights/{controller}/{action}/{id}",
				new { action = "Index", id = UrlParameter.Optional }
			);
		}
	}
}