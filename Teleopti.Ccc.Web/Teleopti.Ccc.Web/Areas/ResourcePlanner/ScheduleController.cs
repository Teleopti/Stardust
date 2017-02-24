using System;
using System.Web.Http;
using Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.Toggle;

namespace Teleopti.Ccc.Web.Areas.ResourcePlanner
{
	public class ScheduleController : ApiController
	{
		private readonly SchedulePlanningPeriodCommandHandler _schedulePlanningPeriodCommandHandler;
		private readonly IToggleManager _toggleManager;

		public ScheduleController(SchedulePlanningPeriodCommandHandler schedulePlanningPeriodCommandHandler, IToggleManager toggleManager)
		{
			_schedulePlanningPeriodCommandHandler = schedulePlanningPeriodCommandHandler;
			_toggleManager = toggleManager;
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
				_schedulePlanningPeriodCommandHandler.Execute(schedulePlanningPeriodCommand);
				return Ok();
			}
			return Ok(_schedulePlanningPeriodCommandHandler.ExecuteAndReturn(schedulePlanningPeriodCommand));
		}
	}

}