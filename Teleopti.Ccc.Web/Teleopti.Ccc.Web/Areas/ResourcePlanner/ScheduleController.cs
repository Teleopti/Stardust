using System;
using System.Web.Http;
using Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner;

namespace Teleopti.Ccc.Web.Areas.ResourcePlanner
{
	public class ScheduleController : ApiController
	{
		private readonly ISchedulePlanningPeriodCommandHandler _schedulePlanningPeriodCommandHandler;


		public ScheduleController(ISchedulePlanningPeriodCommandHandler schedulePlanningPeriodCommandHandler)
		{
			_schedulePlanningPeriodCommandHandler = schedulePlanningPeriodCommandHandler;
		}

		//remove me when we move scheduling/optimization out of http request
		[HttpPost, Route("api/ResourcePlanner/KeepAlive")]
		public virtual void KeepAlive()
		{
		}

		[HttpPost, Route("api/resourceplanner/planningperiod/{planningPeriodId}/schedule")]
		public virtual IHttpActionResult ScheduleForPlanningPeriod(Guid planningPeriodId)
		{
			var schedulePlanningPeriodCommand = new SchedulePlanningPeriodCommand
			{
				PlanningPeriodId = planningPeriodId
			};
			return Ok(_schedulePlanningPeriodCommandHandler.Execute(schedulePlanningPeriodCommand));
		}
	}

}