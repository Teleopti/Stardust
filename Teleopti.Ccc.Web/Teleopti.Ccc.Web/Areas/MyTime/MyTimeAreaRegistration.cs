using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Teleopti.Ccc.Web.Areas.MyTime
{
	public class MyTimeAreaRegistration : AreaRegistration
	{
		public override string AreaName => "MyTime";

		public override void RegisterArea(AreaRegistrationContext context)
		{
			context.Routes.Add(new StaticHtmlRoutesHandler());
			

			var mapRoute = context.MapRoute(
				"MyTime-authentication",
				"MyTime/Authentication/{action}",
				new { controller = "Authentication", action = "SignIn", area = "Start", origin = "MyTime" },
				null,
				new[] { "Teleopti.Ccc.Web.Areas.Start.*" });
			mapRoute.DataTokens["area"] = "Start";

			context.MapRoute(
				"MyTime-widget-alias",
				"MyTime/ASMWidget/{action}",
				new { controller = "Widget", action = "Index" }
				);

			context.MapRoute(
				"MyTime-widget-alias_old",
				"MyTime/CiscoWidget/{action}",
				new { controller = "Widget", action = "Index" }
				);

			context.MapRoute(
				"MyTime-calendar",
				"MyTime/Share",
				new { controller = "ShareCalendar", action = "iCal" }
				);

			context.MapRoute(
				"MyTime-date-route",
				"MyTime/{controller}/{action}/{year}/{month}/{day}",
				new { },
				new { year = @"\d{4}", month = @"\d{2}", day = @"\d{2}" }
				);

			context.MapRoute(
				"MyTime-date-id-route",
				"MyTime/{controller}/{action}/{year}/{month}/{day}/{id}",
				new { id = UrlParameter.Optional },
				new { year = @"\d{4}", month = @"\d{2}", day = @"\d{2}", id = new GuidConstraint() }
				);

			context.MapRoute(
				"MyTime-default",
				"MyTime/{controller}/{action}/{id}",
				new { controller = "Portal", action = "Index", id = UrlParameter.Optional }
				);

		}
	}

	public class StaticHtmlRoutesHandler : RouteBase
	{
		public override RouteData GetRouteData(HttpContextBase httpContext)
		{
			if (httpContext.Request.Url == null)
			{
				return null;
			}
			var url = httpContext.Request.Url.ToString();

			if (Regex.IsMatch(url.ToLower(), @"/mytime/static/.*\.(html|htm)"))
			{
				httpContext.Response.ContentType = "text/html";
				httpContext.Response.TransmitFile($"~/Areas/MyTime/Views/Static/{httpContext.Request.Url.Segments.Last()}");
				httpContext.Response.End();
			}
			return null;
		}

		public override VirtualPathData GetVirtualPath(RequestContext requestContext,
			RouteValueDictionary values)
		{
			return null;
		}
	}
}
