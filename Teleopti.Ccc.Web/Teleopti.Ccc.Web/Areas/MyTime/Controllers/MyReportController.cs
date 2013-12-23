using System.Web.Mvc;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Filters;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.Web.Areas.MyTime.Controllers
{
	[MyReportPermission]
    public class MyReportController : Controller
    {
		[EnsureInPortal]
		public ViewResult Index()
        {
			return View("MyReportPartial");
        }

    }
}
