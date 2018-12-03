using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Domain.WorkflowControl
{
	public class StaffingThresholdValidatorCascadingSkillsWithShrinkage : StaffingThresholdWithShrinkageValidator
	{
		public override IValidatedRequest Validate(IAbsenceRequest absenceRequest, RequiredForHandlingAbsenceRequest requiredForHandlingAbsenceRequest)
		{
			var timeZone = absenceRequest.Person.PermissionInformation.DefaultTimeZone();
			var culture = absenceRequest.Person.PermissionInformation.Culture();
			var uiCulture = absenceRequest.Person.PermissionInformation.UICulture();
			var numberOfRequestedDays = absenceRequest.Period.ToDateOnlyPeriod(timeZone).DayCount();

			var underStaffingResultDict = GetUnderStaffingDays(absenceRequest, requiredForHandlingAbsenceRequest, new CascadingPersonSkillProvider());

			if (underStaffingResultDict.IsNotUnderstaffed())
			{
				return new ValidatedRequest {IsValid = true, ValidationErrors = string.Empty};
			}
			string validationError = numberOfRequestedDays > 1
				? GetUnderStaffingDateString(underStaffingResultDict, culture, uiCulture)
				: GetUnderStaffingPeriodsString(underStaffingResultDict, culture, uiCulture, timeZone);
			return new ValidatedRequest
			{
				IsValid = false,
				ValidationErrors = validationError
			};
		}
	}

	public class StaffingThresholdWithShrinkageValidator : StaffingThresholdValidator
	{
		public override IValidatedRequest Validate(IAbsenceRequest absenceRequest, RequiredForHandlingAbsenceRequest requiredForHandlingAbsenceRequest)
		{
			setUseShrinkage(requiredForHandlingAbsenceRequest.SchedulingResultStateHolder, absenceRequest.Period);
			return base.Validate(absenceRequest, requiredForHandlingAbsenceRequest);
		}

		public override IAbsenceRequestValidator CreateInstance()
		{
			return new StaffingThresholdWithShrinkageValidator();
		}

		public override string DisplayText => Resources.IntradayWithShrinkage;

		public override bool Equals(object obj)
		{
			return obj is StaffingThresholdWithShrinkageValidator;
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int result = (GetType().GetHashCode());
				result = (result*397) ^ (BudgetGroupHeadCountSpecification?.GetHashCode() ?? 0);
				return result;
			}
		}

		private static void setUseShrinkage(ISchedulingResultStateHolder schedulingResultStateHolder, DateTimePeriod period)
		{
			var skillStaffPeriodDictionaries = schedulingResultStateHolder.SkillStaffPeriodHolder.SkillStaffPeriodDictionary(schedulingResultStateHolder.Skills, period).Values;
			foreach (var skillStaffPeriodDictionary in skillStaffPeriodDictionaries)
			{
				skillStaffPeriodDictionary.Values.ForEach(x => x.Payload.UseShrinkage = true);
			}

		}
	}
}