using System;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;

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

	public class ClearScheduleStardustEvent : WebScheduleStardustBaseEvent
	{
	}

	public class WebScheduleStardustEvent : WebScheduleStardustBaseEvent
	{
	}

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