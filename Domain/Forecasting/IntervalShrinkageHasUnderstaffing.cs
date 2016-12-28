using Teleopti.Ccc.Domain.Specification;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting
{
	public class IntervalShrinkageHasUnderstaffing : Specification<IValidatePeriod>
	{
		private readonly ISkill _skill;

		public IntervalShrinkageHasUnderstaffing(ISkill skill)
		{
			_skill = skill;
		}

		public override bool IsSatisfiedBy(IValidatePeriod obj)
		{
			return obj.RelativeDifferenceWithShrinkage < _skill.StaffingThresholds.Understaffing.Value;
		}

	}
}