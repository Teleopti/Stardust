using System.Web.Mvc;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Performance;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Web.Filters;
using Teleopti.Interfaces.Domain;

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

		[UnitOfWorkAction, HttpGet]
		public JsonResult ResetPerformanceCounter(int iterationCount)
		{
			_performanceCounter.Limit = iterationCount;
			_performanceCounter.BusinessUnitId = _businessUnit.Current().Id.GetValueOrDefault();
			_performanceCounter.DataSource = _currentDataSource.CurrentName();
			_performanceCounter.ResetCount();
			return Json("ok", JsonRequestBehavior.AllowGet);
		}

		[UnitOfWorkAction, HttpGet]
		public JsonResult ManageAdherenceLoadTest(int iterationCount)
		{
			var personData = _personGenerator.Generate(iterationCount);
			var stateCodes = _stateGenerator.Generate(iterationCount);
			return Json(new {personData.Persons, States = stateCodes, personData.TeamId}, JsonRequestBehavior.AllowGet);
		}

		[UnitOfWorkAction, HttpGet]
		public JsonResult ClearManageAdherenceLoadTest(int iterationCount)
		{
			_personGenerator.Clear(iterationCount);
			return Json("ok", JsonRequestBehavior.AllowGet);
		}
	}
}