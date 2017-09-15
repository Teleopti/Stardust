using System;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.Web.Areas.ResourcePlanner
{
	[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.WebPlans)]
	public class DayOffRulesController : ApiController
	{
		private readonly IFetchDayOffRulesModel _fetchDayOffRulesModel;
		private readonly IPlanningGroupSettingsModelPersister _planningGroupSettingsModelPersister;

		public DayOffRulesController(IFetchDayOffRulesModel fetchDayOffRulesModel, IPlanningGroupSettingsModelPersister planningGroupSettingsModelPersister)
		{
			_fetchDayOffRulesModel = fetchDayOffRulesModel;
			_planningGroupSettingsModelPersister = planningGroupSettingsModelPersister;
		}

		[UnitOfWork, HttpPost, Route("api/resourceplanner/dayoffrules")]
		public virtual IHttpActionResult Persist(PlanningGroupSettingsModel planningGroupSettingsModel)
		{
			_planningGroupSettingsModelPersister.Persist(planningGroupSettingsModel);
			return Ok();
		}

		[UnitOfWork, HttpGet, Route("api/resourceplanner/dayoffrules")]
		public virtual IHttpActionResult FetchAll()
		{
			return Ok(_fetchDayOffRulesModel.FetchAllWithoutPlanningGroup());
		}

		[UnitOfWork, HttpGet, Route("api/resourceplanner/planninggroup/{planningGroupId}/dayoffrules")]
		public virtual IHttpActionResult FetchAllForPlanningGroup(Guid planningGroupId)
		{
			return Ok(_fetchDayOffRulesModel.FetchAllForPlanningGroup(planningGroupId));
		}

		[UnitOfWork, HttpDelete, Route("api/resourceplanner/dayoffrules/{id}")]
		public virtual IHttpActionResult Delete(Guid id)
		{
			_planningGroupSettingsModelPersister.Delete(id);
			return Ok();
		}

		[UnitOfWork, HttpGet, Route("api/resourceplanner/dayoffrules/{id}")]
		public virtual IHttpActionResult Fetch(Guid id)
		{
			return Ok(_fetchDayOffRulesModel.Fetch(id));
		}
	}
}