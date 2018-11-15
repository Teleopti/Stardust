using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Optimization;
using Owin;
using Teleopti.Ccc.Web.Core.Startup.Booter;

namespace Teleopti.Ccc.Web.Core.Startup.InitializeApplication
{
	[TaskPriority(20)]
	public class BundleScripts : IBootstrapperTask
	{
		public const string MyTimeJs = "~/MyTimeJs";
		public const string BootstrapCss = "~/Content/bootstrap/Content/bundle";
		public const string SignInJs = "~/SignInJs";
		public const string SsoJs = "~/SsoJs";
		//to get correct paths to our pic links in css - need same structure
		public const string Select2Css = "~/Content/select2/bundle";
		public const string MyTimeCss = "~/Areas/MyTime/Content/Css/bundle";
		public const string SignInCss = "~/Areas/Start/Content/Css/bundle"; 
		public const string SsoCss = "~/Areas/SSO/Content/Css/bundle"; 

		public Task Execute(IAppBuilder application)
		{
			BundleTable.Bundles.IgnoreList.Clear();
			BundleTable.Bundles.IgnoreList.Ignore("*Tests.js");

			registerMyTimeScripts();
			registerSigninScripts();
			registerSsoScripts();

			return null;
		}

		

		private static void registerMyTimeScripts()
		{
			registerBootstrapCssIndividualDueToRelativePathsInsideCss();
			registerSelect2CssIndividualDueToRelativePathsInsideCss();
			var cssBundle = new StyleBundle(MyTimeCss)
				.Include(
					"~/Content/moment-datepicker/datepicker.css",
					"~/Content/Scripts/jquery.qtip.css",
					"~/Content/bootstrap-timepicker/css/bootstrap-timepicker.css",
					"~/Content/Scripts/pinify/content/jquery.pinify.min.css",
					"~/Content/jasny-bootstrap/css/jasny-bootstrap.min.css",
					"~/Content/jqueryui/smoothness/jquery-ui.css",
					"~/Content/jqueryui/smoothness/jquery.ui.dialog.css"
				)
				.IncludeDirectory("~/Areas/MyTime/Content/Css", "*.css");
			var jsBundle = new ScriptBundle(MyTimeJs)
				.Include(
					"~/Content/jquery/jquery-1.12.4.js",
					"~/Content/Scripts/knockout-2.2.1.js",
					"~/Content/moment/moment.js",
					"~/Content/moment/moment-with-locales.min.js",
					"~/Content/moment-timezone/moment-timezone-with-data.min.js",
					"~/Content/jalaali-calendar-datepicker/moment-jalaali.js",
					"~/Content/moment-datepicker/moment-datepicker.js",
					"~/Content/moment-datepicker/moment-datepicker-ext.js",
                    "~/Content/jalaali-calendar-datepicker/moment-jalaali-ext.js",
					"~/Content/jalaali-calendar-datepicker/moment-datepicker-jalaali-ext.js",
					"~/Content/moment-datepicker/moment-datepicker-ko.js",
					"~/Content/signals/signals.js",
					"~/Content/hasher/hasher.js",
					"~/Content/crossroads/crossroads.js",
					"~/Content/Scripts/modernizr-2.6.2.js",
					"~/Content/bootstrap/Scripts/bootstrap.js",
					"~/Content/jqueryui/jquery-ui-1.10.2.custom.js",
					"~/Content/jqueryui/jquery-ui-1.10.2.slider.js",
					"~/Content/jqueryui/jquery-ui-1.10.2.dialog.js",
					"~/Content/jqueryui.touch-punch/jquery.ui.touch-punch.js",
					"~/Content/Scripts/json2.js",
					"~/Content/Scripts/jquery.hoverIntent.js",
					"~/Content/Scripts/jquery.qtip.js",
					"~/Content/Scripts/indexOf.js",
					"~/Content/signalr/jquery.signalR-2.3.0.js",
					"~/Content/signalr/broker-hubs.js",
					"~/Content/select2/select2.js",
					"~/Content/bootstrap-timepicker/js/bootstrap-timepicker.js",
					"~/Content/bootstrap-timepicker/js/bootstrap-timepicker-ext.js",
					"~/Content/scripts/pinify/scripts/jquery.pinify.min.js",
					"~/Content/jasny-bootstrap/js/jasny-bootstrap.min.js",
					"~/Content/jquery-plugin/jquery.touchSwipe.min.js"
				)
				.IncludeDirectory("~/Areas/MyTime/Content/Scripts", "*.js", true);
			jsBundle.Orderer = new teleoptiScriptsOrderedByNumberOfDots();
			BundleTable.Bundles.Add(cssBundle);
			BundleTable.Bundles.Add(jsBundle);
		}

		private static void registerSelect2CssIndividualDueToRelativePathsInsideCss()
		{
			var select2Css = new StyleBundle(Select2Css)
				.Include("~/Content/select2/select2.css")
				.Include("~/Content/select2/select2-bootstrap.css");
			BundleTable.Bundles.Add(select2Css);
		}

		private static void registerBootstrapCssIndividualDueToRelativePathsInsideCss()
		{
			var bootstrapCss = new StyleBundle(BootstrapCss)
                .Include("~/Content/bootstrap/Content/bootstrap.css",
                    "~/Content/bootstrap/Content/bootstrap-theme.css");
			BundleTable.Bundles.Add(bootstrapCss);
		}

		private static void registerSigninScripts()
		{
			//use requiresJs here instead?
			var cssBundle = new StyleBundle(SignInCss)
				.Include(
                    "~/Content/bootstrap/Content/bootstrap.css",
                    "~/Content/bootstrap/Content/bootstrap-theme.css",
					"~/Areas/Start/Content/Css/Login.css",
					"~/Areas/Start/Content/Css/Site.css"
				);
			var jsBundle = new ScriptBundle(SignInJs)
				.Include(
					"~/Content/jquery/jquery-1.12.4.js",
					"~/Content/Scripts/jquery.ba-hashchange.js",
					"~/Content/Scripts/modernizr-2.6.2.js",
					"~/Content/bootstrap/Scripts/bootstrap.js",
					"~/Content/Scripts/knockout-2.2.1.js",
					"~/Content/signals/signals.js",
					"~/Content/hasher/hasher.js",
					"~/Content/crossroads/crossroads.js")
				.IncludeDirectory("~/Areas/Start/Content/Scripts", "*.js");
			BundleTable.Bundles.Add(cssBundle);
			BundleTable.Bundles.Add(jsBundle);
		}

		private void registerSsoScripts()
		{
			//use requiresJs here instead?
			var cssBundle = new StyleBundle(SsoCss)
				.Include(
					"~/Content/bootstrap/Content/bootstrap.css",
					"~/Content/bootstrap/Content/bootstrap-theme.css",
					"~/Areas/SSO/Content/Css/Login.css",
					"~/Areas/SSO/Content/Css/Site.css"
				);
			var jsBundle = new ScriptBundle(SsoJs)
				.Include(
					"~/Content/jquery/jquery-1.12.4.js",
					"~/Content/Scripts/jquery.ba-hashchange.js",
					"~/Content/Scripts/modernizr-2.6.2.js",
					"~/Content/bootstrap/Scripts/bootstrap.js",
					"~/Content/Scripts/knockout-2.2.1.js",
					"~/Content/signals/signals.js",
					"~/Content/hasher/hasher.js",
					"~/Content/crossroads/crossroads.js")
				.IncludeDirectory("~/Areas/SSO/Content/Scripts", "*.js");

			BundleTable.Bundles.Add(cssBundle);
			BundleTable.Bundles.Add(jsBundle);
		}

		private class teleoptiScriptsOrderedByNumberOfDots : IBundleOrderer
		{
			public IEnumerable<BundleFile> OrderFiles(BundleContext context, IEnumerable<BundleFile> files)
			{
				const string teleoptiPattern = "Teleopti.";
				var teleoptiScripts = files.Where(x => x.VirtualFile.Name.Contains(teleoptiPattern));
				var teleoptiScriptsOrdered = teleoptiScripts.OrderBy(x => x.VirtualFile.Name.Split('.').Length);
				return files.Except(teleoptiScripts).Concat(teleoptiScriptsOrdered);
			}
		}
	}
}