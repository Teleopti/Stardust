using System.Web.Mvc;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Performance;
using Teleopti.Ccc.Domain.Common;

namespace Teleopti.Ccc.Web.Areas.PerformanceTool.Controllers
{
	public class ApplicationController : Controller
	{
		private readonly IPerformanceCounter _performanceCounter;
		private readonly ICurrentBusinessUnit _businessUnit;
		private readonly ICurrentDataSource _currentDataSource;

		public ApplicationController(IPerformanceCounter performanceCounter, ICurrentBusinessUnit businessUnit, ICurrentDataSource currentDataSource)
		{
			_performanceCounter = performanceCounter;
			_businessUnit = businessUnit;
			_currentDataSource = currentDataSource;
		}

		public ViewResult Index()
		{
			return new ViewResult();
		}

		public JsonResult AdherenceTest(int limit)
		{
			_performanceCounter.Limit = limit;
			_performanceCounter.BusinessUnitId = _businessUnit.Current().Id.GetValueOrDefault();
			_performanceCounter.DataSource = _currentDataSource.CurrentName();
			return Json("Ok", JsonRequestBehavior.AllowGet);
		}
	}
}