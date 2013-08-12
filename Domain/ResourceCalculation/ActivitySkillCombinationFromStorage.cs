using System;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public class ActivitySkillCombinationFromStorage
	{
		public int Id { get; set; }
		public Guid Activity { get; set; }
		public string SkillCombination { get; set; }
		public bool ActivityRequiresSeat { get; set; }
	}
}