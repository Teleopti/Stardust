using System.Threading.Tasks;
using System.Web.Optimization;
using Teleopti.Ccc.Web.Core.Startup.Booter;

namespace Teleopti.Ccc.Web.Core.Startup.InitializeApplication
{
	[TaskPriority(20)]
	public class BundleScripts : IBootstrapperTask
	{
		public Task Execute()
		{
			BundleTable.Bundles.IgnoreList.Ignore("*Tests.js");

			registerMyTimeScripts();
			registerSigninScripts();

			return null;
		}

		private static void registerMyTimeScripts()
		{
			var cssBundle = new StyleBundle("~/MyTimeCss")
				.IncludeDirectory("~/Areas/MyTime/Content/Css", "*.css")
				.Include(
					"~/Content/bootstrap/bootstrap.css",
					"~/Content/moment-datepicker/datepicker.css",
					"~/Content/jqueryui/smoothness/jquery-ui-1.10.2.custom.css",
					"~/Content/Scripts/jquery.qtip.css",
					"~/Content/select2/select2.css",
					"~/Content/bootstrap-timepicker/css/bootstrap-timepicker.min.css",
					"~/Content/Scripts/pinify/content/jquery.pinify.min.css"
				);
			var jsBundle = new ScriptBundle("~/MyTimeJs")
				.Include(
					"~/Content/jquery/jquery-1.10.2.js",
					"~/Content/Scripts/knockout-2.2.1.js",
					"~/Content/moment/moment.js",
					"~/Content/moment/moment.all.js",
					"~/Content/moment-datepicker/moment-datepicker.js",
					"~/Content/moment-datepicker/moment-datepicker-ko.js",
					"~/Content/signals/signals.js",
					"~/Content/hasher/hasher.js",
					"~/Content/crossroads/crossroads.js",
					"~/Content/Scripts/modernizr-2.6.2.js",
					"~/Content/bootstrap/bootstrap.js",
					"~/Content/jqueryui/jquery-ui-1.10.2.custom.js",
					"~/Content/Scripts/json2.js",
					"~/Content/Scripts/jquery.hoverIntent.js",
					"~/Content/jquery-plugin/jquery.placeholder.min.js",
					"~/Content/Scripts/jquery.qtip.js",
					"~/Content/Scripts/indexOf.js",
					"~/Content/signalr/jquery.signalR-1.1.2.js",
					"~/Content/signalr/broker-hubs.js",
					"~/Content/select2/select2.js",
					"~/Content/bootstrap-timepicker/js/bootstrap-timepicker.js",
					"~/Content/scripts/pinify/scripts/jquery.pinify.min.js" //I wonder if this will be read...
				)
				.IncludeDirectory("~/Areas/MyTime/Content/Scripts", "*.js", true);
			BundleTable.Bundles.Add(cssBundle);
			BundleTable.Bundles.Add(jsBundle);
		}

		private static void registerSigninScripts()
		{
			//use requiresJs here instead?
			var cssBundle = new StyleBundle("~/SignInCss")
				.Include(
					"~/Content/bootstrap/bootstrap.css",
					"~/Content/bootstrap/bootstrap-responsive.css",
					"~/Areas/Start/Content/Css/Login.css",
					"~/Areas/Start/Content/Css/Site.css"
				);
			var jsBundle = new ScriptBundle("~/SignInJs")
				.Include(
					"~/Content/jquery/jquery-1.10.2.js",
					"~/Content/Scripts/jquery.ba-hashchange.js",
					"~/Content/Scripts/modernizr-2.6.2.js",
					"~/Content/jquery-plugin/jquery.placeholder.js",
					"~/Content/bootstrap/bootstrap.js",
					"~/Content/Scripts/knockout-2.2.1.js",
					"~/Content/signals/signals.js",
					"~/Content/hasher/hasher.js",
					"~/Content/crossroads/crossroads.js")
				.IncludeDirectory("~/Areas/Start/Content/Scripts", "*.js");
			BundleTable.Bundles.Add(cssBundle);
			BundleTable.Bundles.Add(jsBundle);
		}
	}
}