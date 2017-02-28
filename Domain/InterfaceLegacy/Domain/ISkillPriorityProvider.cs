namespace Teleopti.Interfaces.Domain
{
	public interface ISkillPriorityProvider
	{
		int GetPriority(ISkill skill);
		double GetPriorityValue(ISkill skill);
		Percent GetOverstaffingFactor(ISkill skill);
	}
}