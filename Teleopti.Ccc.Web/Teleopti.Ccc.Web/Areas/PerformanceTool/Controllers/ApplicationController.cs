using System.Web.Mvc;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Performance;

namespace Teleopti.Ccc.Web.Areas.PerformanceTool.Controllers
{
	public class ApplicationController : Controller
	{
		private readonly IPerformanceCounter _performanceCounter;

		public ApplicationController(IPerformanceCounter performanceCounter)
		{
			_performanceCounter = performanceCounter;
		}

		public ViewResult Index()
		{
			return new ViewResult();
		}

		public JsonResult AdherenceTest(int limit)
		{
			_performanceCounter.Limit = limit;
			return Json("Ok", JsonRequestBehavior.AllowGet);
		}
	}
}