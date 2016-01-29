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
		private readonly IActionThrottler _actionThrottler;

		public OptimizationController(ScheduleOptimization scheduleOptimization, IntradayOptimization intradayOptimization, IActionThrottler actionThrottler)
		{
			_scheduleOptimization = scheduleOptimization;
			_intradayOptimization = intradayOptimization;
			_actionThrottler = actionThrottler;
		}

		[HttpPost, Route("api/ResourcePlanner/optimize/FixedStaff/{id}")]
		public virtual IHttpActionResult FixedStaff(Guid id, BlockToken tokenFromScheduling)
		{
			_actionThrottler.Resume(tokenFromScheduling);
			try
			{
				return Ok(_scheduleOptimization.Execute(id));
			}
			finally
			{
				_actionThrottler.Finish(tokenFromScheduling);
			}
		}

		[HttpPost, Route("api/ResourcePlanner/optimize/intraday/{id}")]
		public virtual IHttpActionResult OptimizeIntraday(Guid id, BlockToken tokenFromScheduling)
		{
			_actionThrottler.Resume(tokenFromScheduling);
			try
			{
				return Ok(_intradayOptimization.Optimize(id));
			}
			finally
			{
				_actionThrottler.Finish(tokenFromScheduling);
			}
		}
	}
}