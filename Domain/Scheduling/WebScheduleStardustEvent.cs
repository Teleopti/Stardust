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
		
		public Guid PlanningPeriodId { get; set; }
		public Guid JobResultId { get; set; }
	}

	public class WebClearScheduleStardustEvent : WebScheduleStardustBaseEvent
	{
	}

	[InstancePerLifetimeScope]
	public class WebScheduleStardustEvent : WebScheduleStardustBaseEvent
	{
	}

	public class IntradayOptimizationOnStardustWasOrdered : WebScheduleStardustBaseEvent
	{
	}
}