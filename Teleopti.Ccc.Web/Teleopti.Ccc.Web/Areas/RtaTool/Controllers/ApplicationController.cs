using System.Web.Mvc;

namespace Teleopti.Ccc.Web.Areas.RtaTool.Controllers
{
    public class ApplicationController : Controller
	{
		public RedirectResult Index()
		{
			return Redirect("WFM/#/rtaTool");
		}

		[HttpGet]
		public ViewResult Old()
		{
			return new ViewResult();
		}
	}
}
