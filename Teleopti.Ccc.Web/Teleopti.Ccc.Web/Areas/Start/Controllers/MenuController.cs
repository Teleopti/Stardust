using System.Web.Mvc;
using System.Web.Routing;

namespace Teleopti.Ccc.Web.Areas.Start.Controllers
{
    public class MenuController : Controller
    {
        public ActionResult Index()
        {
        	return RedirectToRoute(new RouteValueDictionary(new {area = "MyTime", controller = "", action = ""}));
        }

    }
}
