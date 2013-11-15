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
			var cssBundle = new StyleBundle("~/MyTimeCss");
			cssBundle.IncludeDirectory("~/Areas/MyTime/Content/Css", "*.css");
			BundleTable.Bundles.Add(cssBundle);
			return null;
		}
	}
}