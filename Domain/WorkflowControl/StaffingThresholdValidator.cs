using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Specification;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.WorkflowControl
{
	public class StaffingThresholdValidator : StaffingThresholdValidatorBase, IAbsenceRequestValidator
	{
		public IAbsenceRequestValidator CreateInstance()
		{
			return new StaffingThresholdValidator();
		}

		public string InvalidReason
		{
			get { return "RequestDenyReasonSkillThreshold"; }
		}

		public string DisplayText
		{
			get { return Resources.Intraday; }
		}

		public override bool Equals(object obj)
		{
			var validator = obj as StaffingThresholdValidator;
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

		protected override Specification<ISkillStaffPeriod> GetIntervalsForUnderstaffing(ISkill skill)
		{
			return new IntervalHasUnderstaffing(skill);
		}

		protected override Specification<ISkillStaffPeriod> GetIntervalsForSeriousUnderstaffing(ISkill skill)
		{
			return new IntervalHasSeriousUnderstaffing(skill);
		}
	}
}