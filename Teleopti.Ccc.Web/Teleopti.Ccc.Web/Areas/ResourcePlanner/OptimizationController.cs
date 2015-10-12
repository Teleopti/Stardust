using System;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Web.Areas.Global;

namespace Teleopti.Ccc.Web.Areas.ResourcePlanner
{
	public class OptimizationController : ApiController
	{
		private readonly ScheduleOptimization _scheduleOptimization;
		private readonly IActionThrottler _actionThrottler;

		public OptimizationController(ScheduleOptimization scheduleOptimization, IActionThrottler actionThrottler)
		{
			_scheduleOptimization = scheduleOptimization;
			_actionThrottler = actionThrottler;
		}

		[HttpPost, Route("api/ResourcePlanner/optimize/FixedStaff/{id}"), Authorize, UnitOfWork]
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
	}
}