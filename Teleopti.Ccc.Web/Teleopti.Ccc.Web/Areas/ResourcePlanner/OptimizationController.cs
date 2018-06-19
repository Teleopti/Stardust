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

		public OptimizationController(IntradayOptimizationFromWeb intradayOptimizationFromWeb)
		{
			_intradayOptimizationFromWeb = intradayOptimizationFromWeb;
		}

		[HttpPost, Route("api/resourceplanner/planningperiod/{planningPeriodId}/optimizeintraday")]
		public void OptimizeIntradayForPlanningPeriod(Guid planningPeriodId, bool runAsynchronously)
		{
			_intradayOptimizationFromWeb.Execute(planningPeriodId, runAsynchronously);
		}
	}
}