using System;
using System.Web.Http;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Areas.Global;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.Web.Areas.ResourcePlanner
{
	[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.WebSchedules)]
	public class OptimizationController : ApiController
	{
		private readonly ScheduleOptimization _scheduleOptimization;
		private readonly IntradayOptimization _intradayOptimization;

		public OptimizationController(ScheduleOptimization scheduleOptimization, IntradayOptimization intradayOptimization)
		{
			_scheduleOptimization = scheduleOptimization;
			_intradayOptimization = intradayOptimization;
		}

		[HttpPost, Route("api/ResourcePlanner/optimize/FixedStaff/{id}")]
		public virtual IHttpActionResult FixedStaff(Guid id)
		{
			return Ok(_scheduleOptimization.Execute(id));
		}

		[HttpPost, Route("api/ResourcePlanner/optimize/intraday/{id}")]
		public virtual IHttpActionResult OptimizeIntraday(Guid id)
		{
			return Ok(_intradayOptimization.Optimize(id));
		}
	}
}