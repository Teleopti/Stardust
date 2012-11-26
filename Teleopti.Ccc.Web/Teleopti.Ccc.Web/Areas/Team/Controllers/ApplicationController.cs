using System.Web.Mvc;

namespace Teleopti.Ccc.Web.Areas.Team.Controllers
{
	public class ApplicationController : Controller
	{
		public FilePathResult Index()
		{
			return new FilePathResult("~/Areas/Team/Content/Templates/index.html", "text/html");
		}
	}
}