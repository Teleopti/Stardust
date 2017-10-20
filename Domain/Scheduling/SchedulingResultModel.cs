using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ResourcePlanner.Validation;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public class SchedulingResultModel
	{
		public int ScheduledAgentsCount { get; set; }
		public IEnumerable<SchedulingValidationError> BusinessRulesValidationResults { get; set; }
	}

	public struct BusinessRulesValidationResult
	{
		public string Name { get; set; }
		public string Message { get; set; }
		public string BusinessRuleCategoryText { get; set; }
	}
}