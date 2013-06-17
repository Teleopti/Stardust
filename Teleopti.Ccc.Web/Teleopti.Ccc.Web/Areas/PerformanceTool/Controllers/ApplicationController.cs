using System.Web.Mvc;

namespace Teleopti.Ccc.Web.Areas.PerformanceTool.Controllers
{
	public class ApplicationController : Controller
	{
		public FilePathResult Index()
		{
			return new FilePathResult("~/Areas/PerformanceTool/Content/Templates/index.html", "text/html");
		}
	}
}