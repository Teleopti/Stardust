using System.Web.Http;
using System.Web.Mvc;

namespace Teleopti.Ccc.Web.Areas.Mart
{
	public class MartAreaRegistration : AreaRegistration
	{
		public override string AreaName
		{
			get
			{
				return "Mart";
			}
		}

		public override void RegisterArea(AreaRegistrationContext context)
		{
			context.Routes.MapHttpRoute(
				name: AreaName + "_API",
				routeTemplate: "api/" + AreaName + "/{controller}/{id}",
				defaults: new { id = RouteParameter.Optional });

			//context.MapRoute(
			//	 "Mart_default",
			//	 "Mart/{controller}/{action}/{id}",
			//	 new { action = "Index", id = UrlParameter.Optional }
			//);
		}
	}
}
