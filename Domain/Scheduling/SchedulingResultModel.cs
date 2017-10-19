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

	public struct BusinessRulesValidationResult : IEquatable<BusinessRulesValidationResult>
	{
		public string Name { get; set; }
		public string Message { get; set; }
		public BusinessRuleCategory BusinessRuleCategory { get; set; }
		public string BusinessRuleCategoryText { get; set; }

		public bool Equals(BusinessRulesValidationResult other)
		{
			return Name.Equals(other.Name) && BusinessRuleCategory.Equals(other.BusinessRuleCategory);
		}

		public override int GetHashCode()
		{
			return Name.GetHashCode() ^ BusinessRuleCategory.GetHashCode();
		}
	}

	public enum BusinessRuleCategory
	{
		DayOff,
		SchedulePeriod
	}
}