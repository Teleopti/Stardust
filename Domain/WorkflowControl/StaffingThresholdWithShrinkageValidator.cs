using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Specification;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.WorkflowControl
{
	public class StaffingThresholdWithShrinkageValidator : StaffingThresholdValidator
	{
		public override IAbsenceRequestValidator CreateInstance()
		{
			return new StaffingThresholdWithShrinkageValidator();
		}

		public override string DisplayText
		{
			get { return Resources.IntradayWithShrinkage; }
		}

		public override bool Equals(object obj)
		{
			var validator = obj as StaffingThresholdWithShrinkageValidator;
			return validator != null;
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int result = (GetType().GetHashCode());
				result = (result * 397) ^ (BudgetGroupHeadCountSpecification != null ? BudgetGroupHeadCountSpecification.GetHashCode() : 0);
				return result;
			}
		}
		
		public override Specification<IValidatePeriod> GetIntervalsForUnderstaffing(ISkill skill)
		{
			return new IntervalShrinkageHasUnderstaffing(skill);
		}

		public override Specification<IValidatePeriod> GetIntervalsForSeriousUnderstaffing(ISkill skill)
		{
			return new IntervalShrinkageHasSeriousUnderstaffing(skill);
		}
	}
}