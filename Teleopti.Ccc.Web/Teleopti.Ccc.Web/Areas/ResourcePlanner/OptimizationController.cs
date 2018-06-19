using System;
using System.Web.Http;
using Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.Web.Areas.ResourcePlanner
{
	[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.WebPlans)]
	public class OptimizationController : ApiController
	{
		private readonly IntradayOptimizationFromWeb _intradayOptimizationFromWeb;
		private readonly IntradayOptimizationOnStardust _intradayOptimizationOnStardust;

		public OptimizationController(IntradayOptimizationFromWeb intradayOptimizationFromWeb,
			IntradayOptimizationOnStardust intradayOptimizationOnStardust)
		{
			_intradayOptimizationFromWeb = intradayOptimizationFromWeb;
			_intradayOptimizationOnStardust = intradayOptimizationOnStardust;
		}

		[HttpPost, Route("api/resourceplanner/planningperiod/{planningPeriodId}/optimizeintraday")]
		public void OptimizeIntradayForPlanningPeriod(Guid planningPeriodId, bool runAsynchronously)
		{
			if (runAsynchronously)
			{
				_intradayOptimizationOnStardust.Execute(planningPeriodId);
			}
			else
			{
				_intradayOptimizationFromWeb.Execute(planningPeriodId, false);
			}
		}
	}
}