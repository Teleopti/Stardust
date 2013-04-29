using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public interface IPersonSkillProvider
	{
		SkillCombination SkillsOnPersonDate(IPerson person, DateOnly date);
	}
}