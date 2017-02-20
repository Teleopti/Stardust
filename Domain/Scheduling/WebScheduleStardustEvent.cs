using System;
using Teleopti.Ccc.Domain.ApplicationLayer;

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

	public class WebOptimizationStardustEvent : WebScheduleStardustBaseEvent
	{
		public WebOptimizationStardustEvent()
		{
		}
		public WebOptimizationStardustEvent(WebScheduleStardustBaseEvent @event) : base(@event)
		{
		}
	}
}