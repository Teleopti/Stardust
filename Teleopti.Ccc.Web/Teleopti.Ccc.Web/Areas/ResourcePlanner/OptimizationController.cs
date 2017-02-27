﻿using System;
using System.Web.Http;
using Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.Web.Areas.ResourcePlanner
{
	[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.WebSchedules)]
	public class OptimizationController : ApiController
	{
		private readonly IScheduleOptimization _scheduleOptimization;
		private readonly IntradayOptimizationFromWeb _intradayOptimizationFromWeb;

		public OptimizationController(IScheduleOptimization scheduleOptimization, IntradayOptimizationFromWeb intradayOptimizationFromWeb)
		{
			_scheduleOptimization = scheduleOptimization;
			_intradayOptimizationFromWeb = intradayOptimizationFromWeb;
		}

		[HttpPost, Route("api/resourceplanner/planningperiod/{planningPeriodId}/optimize")]
		public virtual IHttpActionResult OptimizeForPlanningPeriod(Guid planningPeriodId)
		{
			return Ok(_scheduleOptimization.Execute(planningPeriodId));
		}

		[HttpPost, Route("api/resourceplanner/planningperiod/{planningPeriodId}/optimizeintraday")]
		public void OptimizeIntradayForPlanningPeriod(Guid planningPeriodId)
		{
			_intradayOptimizationFromWeb.Execute(planningPeriodId);
		}
	}
}