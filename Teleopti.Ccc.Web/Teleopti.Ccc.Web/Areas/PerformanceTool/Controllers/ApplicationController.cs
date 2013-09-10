using System.Web.Mvc;

namespace Teleopti.Ccc.Web.Areas.PerformanceTool.Controllers
{
	public class ApplicationController : Controller
	{
		public ViewResult Index()
		{
			return new ViewResult();
			//return new FilePathResult("~/Areas/PerformanceTool/Content/Templates/index.html", "text/html");
		}
	}
}