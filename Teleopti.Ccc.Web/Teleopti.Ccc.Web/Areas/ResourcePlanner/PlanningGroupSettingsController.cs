﻿using System;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.Web.Areas.ResourcePlanner
{
	[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.WebPlans)]
	public class PlanningGroupSettingsController : ApiController
	{
		private readonly IFetchPlanningGroupSettingsModel _fetchPlanningGroupSettingsModel;
		private readonly IPlanningGroupSettingsModelPersister _planningGroupSettingsModelPersister;

		public PlanningGroupSettingsController(IFetchPlanningGroupSettingsModel fetchPlanningGroupSettingsModel, IPlanningGroupSettingsModelPersister planningGroupSettingsModelPersister)
		{
			_fetchPlanningGroupSettingsModel = fetchPlanningGroupSettingsModel;
			_planningGroupSettingsModelPersister = planningGroupSettingsModelPersister;
		}

		[UnitOfWork, HttpPost, Route("api/resourceplanner/plangroupsetting")] 
		public virtual IHttpActionResult Persist(PlanningGroupSettingsModel planningGroupSettingsModel)
		{
			_planningGroupSettingsModelPersister.Persist(planningGroupSettingsModel);
			return Ok();
		}

		[UnitOfWork, HttpGet, Route("api/resourceplanner/planninggroup/{planningGroupId}/plangroupsetting")] 
		public virtual IHttpActionResult FetchAllForPlanningGroup(Guid planningGroupId)
		{
			return Ok(_fetchPlanningGroupSettingsModel.FetchAllForPlanningGroup(planningGroupId));
		}

		[UnitOfWork, HttpDelete, Route("api/resourceplanner/plangroupsetting/{id}")] 
		public virtual IHttpActionResult Delete(Guid id)
		{
			_planningGroupSettingsModelPersister.Delete(id);
			return Ok();
		}

		[UnitOfWork, HttpGet, Route("api/resourceplanner/plangroupsetting/{id}")] 
		public virtual IHttpActionResult Fetch(Guid id)
		{
			return Ok(_fetchPlanningGroupSettingsModel.Fetch(id));
		}
	}
}