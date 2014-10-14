using System.Web.Mvc;

namespace Teleopti.Ccc.Web.Areas.Forecasting
{
	public class ForecastingAreaRegistration : AreaRegistration
	{
		public override void RegisterArea(AreaRegistrationContext context)
		{
			context.MapRoute(
				"Forecasting-default",
				"Forecasting/{controller}/{action}",
				new { controller = "Application", action = "Index" }
				);
		}

		public override string AreaName
		{
			get { return "Forecasting"; }
		}
	}
}