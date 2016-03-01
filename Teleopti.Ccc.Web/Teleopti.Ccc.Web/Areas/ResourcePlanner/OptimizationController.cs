using System;
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
		private readonly ScheduleOptimization _scheduleOptimization;
		private readonly IntradayOptimizationCommandHandler _intradayOptimizationCommandHandler;

		public OptimizationController(ScheduleOptimization scheduleOptimization, IntradayOptimizationCommandHandler intradayOptimizationCommandHandler)
		{
			_scheduleOptimization = scheduleOptimization;
			_intradayOptimizationCommandHandler = intradayOptimizationCommandHandler;
		}

		[HttpPost, Route("api/ResourcePlanner/optimize/FixedStaff/{id}")]
		public virtual IHttpActionResult FixedStaff(Guid id)
		{
			return Ok(_scheduleOptimization.Execute(id));
		}

		[HttpPost, Route("api/ResourcePlanner/optimize/intraday/{id}")]
		public virtual IHttpActionResult OptimizeIntraday(Guid id)
		{
			return Ok(_intradayOptimizationCommandHandler.Execute(id));
		}
	}
}