using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public interface IPersonSkillProvider
	{
		SkillCombination SkillsOnPersonDate(IPerson person, DateOnly date);
	}
}