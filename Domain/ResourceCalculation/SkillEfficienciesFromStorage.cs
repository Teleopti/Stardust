using System;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public class SkillEfficienciesFromStorage
	{
		public long ParentId { get; set; }
		public Guid SkillId { get; set; }
		public double Amount { get; set; }
	}
}