using Teleopti.Ccc.Domain.Specification;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting
{
	public class IntervalShrinkageHasSeriousUnderstaffing : Specification<IValidatePeriod>
	{
		private readonly ISkill _skill;

		public IntervalShrinkageHasSeriousUnderstaffing(ISkill skill)
		{
			_skill = skill;
		}

		public override bool IsSatisfiedBy(IValidatePeriod obj)
		{
			return obj.RelativeDifferenceWithShrinkage < _skill.StaffingThresholds.SeriousUnderstaffing.Value;
		}
	}
}