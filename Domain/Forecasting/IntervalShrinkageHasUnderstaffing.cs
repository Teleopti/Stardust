using Teleopti.Ccc.Domain.Specification;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting
{
	public class IntervalShrinkageHasUnderstaffing : Specification<ISkillStaffPeriod>
	{
		private readonly ISkill _skill;

		public IntervalShrinkageHasUnderstaffing(ISkill skill)
		{
			_skill = skill;
		}

		public override bool IsSatisfiedBy(ISkillStaffPeriod obj)
		{
			return obj.RelativeDifferenceWithShrinkage < _skill.StaffingThresholds.Understaffing.Value;
		}

	}
}