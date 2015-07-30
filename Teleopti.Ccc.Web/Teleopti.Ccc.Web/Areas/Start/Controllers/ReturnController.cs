using System.Web.Mvc;

namespace Teleopti.Ccc.Web.Areas.Start.Controllers
{
	[OutputCache(NoStore = true, Duration = 0, VaryByParam = "None")]
	public class ReturnController : Controller
	{
		public ViewResult Hash()
		{
			return View();
		}
	}
}