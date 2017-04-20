﻿using System;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.Web.Areas.ResourcePlanner
{
	[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.WebSchedules)]
	public class DayOffRulesController : ApiController
	{
		private readonly IFetchDayOffRulesModel _fetchDayOffRulesModel;
		private readonly IDayOffRulesModelPersister _dayOffRulesModelPersister;

		public DayOffRulesController(IFetchDayOffRulesModel fetchDayOffRulesModel, IDayOffRulesModelPersister dayOffRulesModelPersister)
		{
			_fetchDayOffRulesModel = fetchDayOffRulesModel;
			_dayOffRulesModelPersister = dayOffRulesModelPersister;
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
			return Ok(_fetchDayOffRulesModel.FetchAllWithoutAgentGroup());
		}

		[UnitOfWork, HttpGet, Route("api/resourceplanner/agentgroup/{agentGroupId}/dayoffrules")]
		public virtual IHttpActionResult FetchAllForAgentGroup(Guid agentGroupId)
		{
			return Ok(_fetchDayOffRulesModel.FetchAllForAgentGroup(agentGroupId));
		}

		[UnitOfWork, HttpDelete, Route("api/resourceplanner/dayoffrules/{id}")]
		public virtual IHttpActionResult Delete(Guid id)
		{
			_dayOffRulesModelPersister.Delete(id);
			return Ok();
		}

		[UnitOfWork, HttpGet, Route("api/resourceplanner/dayoffrules/{id}")]
		public virtual IHttpActionResult Fetch(Guid id)
		{
			return Ok(_fetchDayOffRulesModel.Fetch(id));
		}
	}
}