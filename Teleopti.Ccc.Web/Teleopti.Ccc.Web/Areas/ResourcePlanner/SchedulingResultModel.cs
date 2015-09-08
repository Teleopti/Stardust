using System;
using System.Collections.Generic;
using Teleopti.Ccc.Web.Areas.Global;

namespace Teleopti.Ccc.Web.Areas.ResourcePlanner
{
	public class SchedulingResultModel
	{
		public int DaysScheduled { get; set; }
		public int ConflictCount { get; set; }
		public int ScheduledAgentsCount { get; set; }
		public IEnumerable<BusinessRulesValidationResult> BusinessRulesValidationResults { get; set; }
		public BlockToken ThrottleToken { get; set; }
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