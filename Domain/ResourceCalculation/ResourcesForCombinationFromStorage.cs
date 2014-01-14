using System;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public class ResourcesForCombinationFromStorage
	{
		public long Id { get; set; }
		public int ActivitySkillCombinationId { get; set; }
		public double Resources { get; set; }
		public double Heads { get; set; }
		public DateTime PeriodStart { get; set; }
		public DateTime PeriodEnd { get; set; }
	}
}