﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Optimization;
using Teleopti.Ccc.Web.Core.Startup.Booter;

namespace Teleopti.Ccc.Web.Core.Startup.InitializeApplication
{
	[TaskPriority(20)]
	public class BundleScripts : IBootstrapperTask
	{
		public const string MyTimeJs = "~/MyTimeJs";
		public const string BootstrapCss = "~/Content/bootstrap/bundle";
		public const string SignInJs = "~/SignInJs";
		//to get correct paths to our pic links in css - need same structure
		public const string Select2Css = "~/Content/select2/bundle";
		public const string MyTimeCss = "~/Areas/MyTime/Content/Css/bundle";
		public const string SignInCss = "~/Areas/Start/Content/Css/bundle"; 

		public Task Execute()
		{
			BundleTable.Bundles.IgnoreList.Clear();
			BundleTable.Bundles.IgnoreList.Ignore("*Tests.js");

			registerMyTimeScripts();
			registerSigninScripts();

			return null;
		}

		private static void registerMyTimeScripts()
		{
			registerBootstrapCssIndividualDueToRelativePathsInsideCss();
			registerSelect2CssIndividualDueToRelativePathsInsideCss();
			var cssBundle = new StyleBundle(MyTimeCss)
				.Include(
					"~/Content/moment-datepicker/datepicker.css",
					"~/Content/jqueryui/smoothness/jquery-ui-1.10.2.custom.css",
					"~/Content/Scripts/jquery.qtip.css",
					"~/Content/bootstrap-timepicker/css/bootstrap-timepicker.css",
					"~/Content/Scripts/pinify/content/jquery.pinify.min.css"
				)
				.IncludeDirectory("~/Areas/MyTime/Content/Css", "*.css");
			var jsBundle = new ScriptBundle(MyTimeJs)
				.Include(
					"~/Content/jquery/jquery-1.10.2.js",
					"~/Content/Scripts/knockout-2.2.1.js",
					"~/Content/moment/moment.js",
					"~/Content/moment/moment.all.min.js",
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
					"~/Content/signalr/jquery.signalR-1.1.4.js",
					"~/Content/signalr/broker-hubs.js",
					"~/Content/select2/select2.js",
					"~/Content/bootstrap-timepicker/js/bootstrap-timepicker.js",
					"~/Content/scripts/pinify/scripts/jquery.pinify.min.js"
				)
				.IncludeDirectory("~/Areas/MyTime/Content/Scripts", "*.js", true);
			jsBundle.Orderer = new teleoptiScriptsOrderedByNumberOfDots();
			BundleTable.Bundles.Add(cssBundle);
			BundleTable.Bundles.Add(jsBundle);
		}

		private static void registerSelect2CssIndividualDueToRelativePathsInsideCss()
		{
			var select2Css = new StyleBundle(Select2Css)
				.Include("~/Content/select2/select2.css");
			BundleTable.Bundles.Add(select2Css);
		}

		private static void registerBootstrapCssIndividualDueToRelativePathsInsideCss()
		{
			var bootstrapCss = new StyleBundle(BootstrapCss)
				.Include("~/Content/bootstrap/bootstrap.css",
					"~/Content/bootstrap/bootstrap-responsive.css");
			BundleTable.Bundles.Add(bootstrapCss);
		}

		private static void registerSigninScripts()
		{
			//use requiresJs here instead?
			var cssBundle = new StyleBundle(SignInCss)
				.Include(
					"~/Content/bootstrap/bootstrap.css",
					"~/Content/bootstrap/bootstrap-responsive.css",
					"~/Areas/Start/Content/Css/Login.css",
					"~/Areas/Start/Content/Css/Site.css"
				);
			var jsBundle = new ScriptBundle(SignInJs)
				.Include(
					"~/Content/jquery/jquery-1.10.2.js",
					"~/Content/Scripts/jquery.ba-hashchange.js",
					"~/Content/Scripts/modernizr-2.6.2.js",
					"~/Content/bootstrap/bootstrap.js",
					"~/Content/Scripts/knockout-2.2.1.js",
					"~/Content/signals/signals.js",
					"~/Content/hasher/hasher.js",
					"~/Content/crossroads/crossroads.js")
				.IncludeDirectory("~/Areas/Start/Content/Scripts", "*.js");
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