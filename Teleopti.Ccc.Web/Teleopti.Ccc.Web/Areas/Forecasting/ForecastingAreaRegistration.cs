using System.Web.Http;
using System.Web.Mvc;

namespace Teleopti.Ccc.Web.Areas.Forecasting
{
	public class ForecastingAreaRegistration : AreaRegistration
	{
		public override void RegisterArea(AreaRegistrationContext context)
		{
			context.Routes.MapHttpRoute(
				name: AreaName + "_API",
				routeTemplate: "api/" + AreaName + "/{controller}/{id}",
				defaults: new {id = RouteParameter.Optional});
		}

		public override string AreaName
		{
			get { return "Forecasting"; }
		}
	}
}