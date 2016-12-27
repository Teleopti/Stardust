using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Specification;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.WorkflowControl
{
	public class StaffingThresholdValidatorCascadingSkillsWithShrinkage : StaffingThresholdWithShrinkageValidator
	{
		public override IValidatedRequest Validate(IAbsenceRequest absenceRequest, RequiredForHandlingAbsenceRequest requiredForHandlingAbsenceRequest)
		{
			var timeZone = absenceRequest.Person.PermissionInformation.DefaultTimeZone();
			var culture = absenceRequest.Person.PermissionInformation.Culture();
			var uiCulture = absenceRequest.Person.PermissionInformation.UICulture();
			var numberOfRequestedDays = absenceRequest.Period.ToDateOnlyPeriod(timeZone).DayCollection().Count;

			var staffingThresholdValidatorHelper = new StaffingThresholdValidatorHelper(GetIntervalsForUnderstaffing, GetIntervalsForSeriousUnderstaffing);

			var underStaffingResultDict = staffingThresholdValidatorHelper.GetUnderStaffingDays(absenceRequest, requiredForHandlingAbsenceRequest, new CascadingPersonSkillProvider());

			if (underStaffingResultDict.IsNotUnderstaffed())
			{
				return new ValidatedRequest { IsValid = true, ValidationErrors = string.Empty };
			}
			string validationError = numberOfRequestedDays > 1
				? GetUnderStaffingDateString(underStaffingResultDict, culture, uiCulture)
				: GetUnderStaffingHourString(underStaffingResultDict, culture, uiCulture, timeZone, absenceRequest.Period.StartDateTimeLocal(timeZone));
			return new ValidatedRequest
			{
				IsValid = false,
				ValidationErrors = validationError
			};
		}
	}

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
		
		public override Specification<ISkillStaffPeriod> GetIntervalsForUnderstaffing(ISkill skill)
		{
			return new IntervalShrinkageHasUnderstaffing(skill);
		}

		public override Specification<ISkillStaffPeriod> GetIntervalsForSeriousUnderstaffing(ISkill skill)
		{
			return new IntervalShrinkageHasSeriousUnderstaffing(skill);
		}
	}
}