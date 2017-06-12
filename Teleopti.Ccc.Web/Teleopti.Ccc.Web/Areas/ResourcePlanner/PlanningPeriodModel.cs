using System;

namespace Teleopti.Ccc.Web.Areas.ResourcePlanner
{
	public class PlanningPeriodModel
	{
		public Guid Id { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public bool HasNextPlanningPeriod { get; set; }
		public string State { get; set; }
		public Guid? PlanningGroupId { get; set; }
		public int TotalAgents { get; set; }
	}
}