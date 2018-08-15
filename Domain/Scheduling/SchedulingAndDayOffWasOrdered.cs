using System;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.FeatureFlags;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public abstract class WebScheduleStardustBaseEvent : StardustJobInfo
	{
		public static string HalfNodesAffinity = "HalfNodesAffinity";

		protected WebScheduleStardustBaseEvent()
		{
			Policy = HalfNodesAffinity;
		}
		protected WebScheduleStardustBaseEvent(WebScheduleStardustBaseEvent @event):this()
		{
			PlanningPeriodId = @event.PlanningPeriodId;
		}
		public Guid PlanningPeriodId { get; set; }
		public Guid JobResultId { get; set; }
	}

	public class WebClearScheduleStardustEvent : WebScheduleStardustBaseEvent
	{
	}

	public class SchedulingAndDayOffWasOrdered : WebScheduleStardustBaseEvent
	{
	}

	[RemoveMeWithToggle(Toggles.ResourcePlanner_MergeSchedulingAndDO_76496)]
	public class WebDayoffOptimizationStardustEvent : WebScheduleStardustBaseEvent
	{
		public WebDayoffOptimizationStardustEvent()
		{
		}
		public WebDayoffOptimizationStardustEvent(WebScheduleStardustBaseEvent @event) : base(@event)
		{
		}
	}

	public class IntradayOptimizationOnStardustWasOrdered : WebScheduleStardustBaseEvent
	{
	}
}