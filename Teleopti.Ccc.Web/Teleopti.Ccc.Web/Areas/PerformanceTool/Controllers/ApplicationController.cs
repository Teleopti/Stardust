using System.Web.Mvc;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Performance;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.PerformanceTool;

namespace Teleopti.Ccc.Web.Areas.PerformanceTool.Controllers
{
	public class ApplicationController : Controller
	{
		private readonly IPerformanceCounter _performanceCounter;
		private readonly ICurrentBusinessUnit _businessUnit;
		private readonly ICurrentDataSource _currentDataSource;
		private readonly IPersonGenerator _personGenerator;
		private readonly IStateGenerator _stateGenerator;

		public ApplicationController(IPerformanceCounter performanceCounter,
			ICurrentBusinessUnit businessUnit,
			ICurrentDataSource currentDataSource,
			IPersonGenerator personGenerator,
			IStateGenerator stateGenerator)
		{
			_performanceCounter = performanceCounter;
			_businessUnit = businessUnit;
			_currentDataSource = currentDataSource;
			_personGenerator = personGenerator;
			_stateGenerator = stateGenerator;
		}

		public ViewResult Index()
		{
			return new ViewResult();
		}

		public JsonResult AdherenceTest(int limit)
		{
			return Json("Ok", JsonRequestBehavior.AllowGet);
		}


		public JsonResult ManageAdherenceLoadTest(int iterationCount)
		{
			_performanceCounter.Limit = iterationCount;
			_performanceCounter.BusinessUnitId = _businessUnit.Current().Id.GetValueOrDefault();
			_performanceCounter.DataSource = _currentDataSource.CurrentName();
			_performanceCounter.ResetCount();
			return null;
		}
	}
}