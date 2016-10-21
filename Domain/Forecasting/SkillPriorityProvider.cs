using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting
{
	public class SkillPriorityProvider : ISkillPriorityProvider
	{
		public int GetPriority(ISkill skill)
		{
			return ((ISkillPriority) skill).Priority;
		}

		public double GetPriorityValue(ISkill skill)
		{
			return ((ISkillPriority)skill).PriorityValue;
		}

		public Percent GetOverstaffingFactor(ISkill skill)
		{
			return ((ISkillPriority)skill).OverstaffingFactor;
		}
	}

	public class SkillPriorityProviderForToggle41312 : ISkillPriorityProvider
	{
		public int GetPriority(ISkill skill)
		{
			return 4;
		}

		public double GetPriorityValue(ISkill skill)
		{
			return 1;
		}

		public Percent GetOverstaffingFactor(ISkill skill)
		{
			return new Percent(.5);
		}
	}
}