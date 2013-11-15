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
			BundleTable.Bundles.Add(cssBundle);
			return null;
		}
	}
}