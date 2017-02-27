using System;
using System.Web.Http;
using Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.Toggle;

namespace Teleopti.Ccc.Web.Areas.ResourcePlanner
{
	public class ScheduleController : ApiController
	{
		private readonly IToggleManager _toggleManager;
		private readonly SchedulePlanningPeriodCommandHandler _schedulePlanningPeriodCommandHandler;
		private readonly SchedulePlanningPeriodTaskCommandHandler _schedulePlanningPeriodTaskCommandHandler;


		public ScheduleController(IToggleManager toggleManager, SchedulePlanningPeriodCommandHandler schedulePlanningPeriodCommandHandler, SchedulePlanningPeriodTaskCommandHandler schedulePlanningPeriodTaskCommandHandler)
		{
			_toggleManager = toggleManager;
			_schedulePlanningPeriodCommandHandler = schedulePlanningPeriodCommandHandler;
			_schedulePlanningPeriodTaskCommandHandler = schedulePlanningPeriodTaskCommandHandler;
		}

		//remove me when we move scheduling/optimization out of http request
		[HttpPost, Route("api/ResourcePlanner/KeepAlive")]
		public virtual void KeepAlive()
		{
		}

		[HttpPost, Route("api/ResourcePlanner/Schedule/{id}")]
		public virtual IHttpActionResult ScheduleForPlanningPeriod(Guid id)
		{
			var toggle = _toggleManager.IsEnabled(Toggles.Wfm_ResourcePlanner_SchedulingOnStardust_42874);
			var schedulePlanningPeriodCommand = new SchedulePlanningPeriodCommand
			{
				PlanningPeriodId = id
			};
			if (toggle)
			{
				_schedulePlanningPeriodTaskCommandHandler.Execute(schedulePlanningPeriodCommand);
				return Ok();
			}
			return Ok(_schedulePlanningPeriodCommandHandler.Execute(schedulePlanningPeriodCommand));
		}
	}

}