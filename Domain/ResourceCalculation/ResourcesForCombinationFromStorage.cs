using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public class ResourcesForCombinationFromStorage
	{
		public int ActivitySkillCombinationId { get; set; }
		public double Resources { get; set; }
		public double Heads { get; set; }
		public DateTimePeriod Period { get; set; }
	}
}