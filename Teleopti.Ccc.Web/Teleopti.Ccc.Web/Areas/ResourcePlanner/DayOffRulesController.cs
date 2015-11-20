using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.Web.Areas.ResourcePlanner
{
	public class DayOffRulesController : ApiController
	{
		private readonly IFetchDayOffRulesModel _fetchDayOffRulesModel;
		private readonly IDayOffRulesModelPersister _dayOffRulesModelPersister;

		public DayOffRulesController(IFetchDayOffRulesModel fetchDayOffRulesModel, IDayOffRulesModelPersister dayOffRulesModelPersister)
		{
			_fetchDayOffRulesModel = fetchDayOffRulesModel;
			_dayOffRulesModelPersister = dayOffRulesModelPersister;
		}


		//TODO: remove this (under objects it uses when old "schedule view" is gone and instead filters are used.
		[UnitOfWork, HttpGet, Route("api/resourceplanner/dayoffrules/default")]
		public virtual IHttpActionResult GetDefaultSettings()
		{
			return Ok(_fetchDayOffRulesModel.FetchDefaultRules());
		}

		[UnitOfWork, HttpPost, Route("api/resourceplanner/dayoffrules")]
		public virtual IHttpActionResult Persist(DayOffRulesModel dayOffRulesModel)
		{
			_dayOffRulesModelPersister.Persist(dayOffRulesModel);
			return Ok();
		}

		[UnitOfWork, HttpGet, Route("api/resourceplanner/dayoffrules")]
		public virtual IHttpActionResult FetchAll()
		{
			return Ok(_fetchDayOffRulesModel.FetchAll());
		}
	}
}