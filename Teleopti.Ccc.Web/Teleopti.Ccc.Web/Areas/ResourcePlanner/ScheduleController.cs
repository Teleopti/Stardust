using System;
using System.Web.Http;
using Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.Web.Areas.ResourcePlanner
{
	[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.WebPlans)]
	public class ScheduleController : ApiController
	{
		private readonly SchedulePlanningPeriodCommandHandler _schedulePlanningPeriodCommandHandler;
		private readonly ClearPlanningPeriodSchedulingCommandHandler _clearPlanningPeriodSchedulingCommandHandler;

		public ScheduleController(SchedulePlanningPeriodCommandHandler schedulePlanningPeriodCommandHandler, ClearPlanningPeriodSchedulingCommandHandler clearPlanningPeriodSchedulingCommandHandler)
		{
			_schedulePlanningPeriodCommandHandler = schedulePlanningPeriodCommandHandler;
			_clearPlanningPeriodSchedulingCommandHandler = clearPlanningPeriodSchedulingCommandHandler;
		}

		//remove me when we move scheduling/optimization out of http request
		[HttpPost, Route("api/ResourcePlanner/KeepAlive")]
		public virtual void KeepAlive()
		{
		}

		[HttpPost, Route("api/resourceplanner/planningperiod/{planningPeriodId}/schedule")]
		public virtual IHttpActionResult ScheduleForPlanningPeriod(Guid planningPeriodId)
		{
			return Ok(_schedulePlanningPeriodCommandHandler.Execute(planningPeriodId));
		}

		[HttpDelete, Route("api/resourceplanner/planningperiod/{planningPeriodId}/schedule")]
		public virtual IHttpActionResult ClearSchedulesForPlanningPeriod(Guid planningPeriodId)
		{
			_clearPlanningPeriodSchedulingCommandHandler.ClearSchedules(new ClearPlanningPeriodSchedulingCommand
			{
				PlanningPeriodId = planningPeriodId
			});
			return Ok();
		}
	}
}