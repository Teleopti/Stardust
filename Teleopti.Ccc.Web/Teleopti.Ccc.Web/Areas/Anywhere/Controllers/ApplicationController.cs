using System.Web.Mvc;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Controllers
{
	public class ApplicationController : Controller
	{
		public FilePathResult Index()
		{
			return new FilePathResult("~/Areas/Anywhere/Content/Templates/index.html", "text/html");
		}
	}
}