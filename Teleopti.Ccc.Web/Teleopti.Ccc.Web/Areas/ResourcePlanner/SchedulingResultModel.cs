using System.Collections.Generic;

namespace Teleopti.Ccc.Web.Areas.ResourcePlanner
{
	public class SchedulingResultModel
	{
		public int DaysScheduled { get; set; }
		public int ConflictCount { get; set; }
		public IEnumerable<BusinessRulesValidationResult> BusinessRulesValidationResults { get; set; }
	}

	public struct BusinessRulesValidationResult
	{
		public string Name { get; set; }
		public string Message { get; set; }
		public BusinessRuleCategory BusinessRuleCategory { get; set; }
	}

	public enum BusinessRuleCategory
	{
		DayOff,
		SchedulePeriod
	}
}