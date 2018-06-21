using System;
using System.Web.Http;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.Web.Areas.ResourcePlanner
{
	[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.WebPlans)]
	public class OptimizationController : ApiController
	{
		private readonly IntradayOptimizationOnStardust _intradayOptimizationOnStardust;

		public OptimizationController(IntradayOptimizationOnStardust intradayOptimizationOnStardust)
		{
			_intradayOptimizationOnStardust = intradayOptimizationOnStardust;
		}

		[HttpPost, Route("api/resourceplanner/planningperiod/{planningPeriodId}/optimizeintraday")]
		public void OptimizeIntradayForPlanningPeriod(Guid planningPeriodId)
		{
			_intradayOptimizationOnStardust.Execute(planningPeriodId);
		}
	}
}