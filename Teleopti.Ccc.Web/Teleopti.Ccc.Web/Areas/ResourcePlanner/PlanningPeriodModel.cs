using System;
using Teleopti.Ccc.Web.Areas.ResourcePlanner.Validation;

namespace Teleopti.Ccc.Web.Areas.ResourcePlanner
{
	public class PlanningPeriodModel
	{
		public Guid Id { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public bool HasNextPlanningPeriod { get; set; }
		public string State { get; set; }
		public ValidationResult ValidationResult { get; set; }
		public Guid? AgentGroupId { get; set; }
	}
	
}