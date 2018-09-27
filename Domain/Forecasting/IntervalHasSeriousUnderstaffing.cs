using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Specification;

namespace Teleopti.Ccc.Domain.Forecasting
{
	public class IntervalHasSeriousUnderstaffing : Specification<IValidatePeriod>
	{
		private readonly ISkill _skill;

		public IntervalHasSeriousUnderstaffing(ISkill skill)
		{
			_skill = skill;
		}

		public override bool IsSatisfiedBy(IValidatePeriod obj)
		{
			var seriousUnderstaffing = _skill.StaffingThresholds.SeriousUnderstaffing.Value;
			var isSatisfied = obj.RelativeDifference < seriousUnderstaffing;
			return isSatisfied;
		}
	}
}