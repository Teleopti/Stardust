using System.Collections.Generic;
using Teleopti.Ccc.Domain.ResourcePlanner.Hints;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public class SchedulingResultModel
	{
		public int ScheduledAgentsCount { get; set; }
		public IEnumerable<SchedulingHintError> BusinessRulesValidationResults { get; set; }
	}

	public struct BusinessRulesValidationResult
	{
		public string Name { get; set; }
		public string Message { get; set; }
		public string BusinessRuleCategoryText { get; set; }
	}
}