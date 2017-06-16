using System;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public abstract class WebScheduleStardustBaseEvent : EventWithInfrastructureContext
	{
		protected WebScheduleStardustBaseEvent()
		{
			
		}
		protected WebScheduleStardustBaseEvent(WebScheduleStardustBaseEvent @event)
		{
			PlanningPeriodId = @event.PlanningPeriodId;
		}
		public Guid PlanningPeriodId { get; set; }
		public Guid JobResultId { get; set; }
	}

	public class WebScheduleStardustEvent : WebScheduleStardustBaseEvent
	{
		public WebScheduleStardustEvent()
		{
		}
		public WebScheduleStardustEvent(WebScheduleStardustBaseEvent @event) : base(@event)
		{
		}

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

	public class WebIntradayOptimizationStardustEvent : WebScheduleStardustBaseEvent
	{
		public IntradayOptimizationWasOrdered IntradayOptimizationWasOrdered { get; set; }
		public int TotalEvents { get; set; }
	}
}