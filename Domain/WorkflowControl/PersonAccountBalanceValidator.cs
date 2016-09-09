using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.WorkflowControl
{
	public class PersonAccountBalanceValidator : IAbsenceRequestValidator
	{
		private string _invalidReason = "RequestDenyReasonPersonAccount";

		public IBudgetGroupHeadCountSpecification BudgetGroupHeadCountSpecification { get; set; }

		public string InvalidReason
		{
			get { return _invalidReason; }
		}

		public string DisplayText
		{
			get { return UserTexts.Resources.Yes; }
		}

		public IValidatedRequest Validate(IAbsenceRequest absenceRequest,
			RequiredForHandlingAbsenceRequest requiredForHandlingAbsenceRequest)
		{
			InParameter.NotNull("SchedulingResultStateHolder", requiredForHandlingAbsenceRequest.SchedulingResultStateHolder);
			InParameter.NotNull("PersonAccountBalanceCalculator",
				requiredForHandlingAbsenceRequest.PersonAccountBalanceCalculator);

			var person = absenceRequest.Person;

			var validatedRequest = new ValidatedRequest();
			validatedRequest.IsValid =
				requiredForHandlingAbsenceRequest.PersonAccountBalanceCalculator.CheckBalance(
					requiredForHandlingAbsenceRequest.SchedulingResultStateHolder.Schedules[person],
					absenceRequest.Period.ToDateOnlyPeriod(person.PermissionInformation.DefaultTimeZone()));

			if (!validatedRequest.IsValid)
			{
				if (waitlistingIsEnabled(absenceRequest))
					_invalidReason = "RequestWaitlistedReasonPersonAccount";
				validatedRequest.ValidationErrors =
					UserTexts.Resources.ResourceManager.GetString(_invalidReason,
						person.PermissionInformation.UICulture());
			}
			return validatedRequest;
		}

		public IAbsenceRequestValidator CreateInstance()
		{
			return new PersonAccountBalanceValidator();
		}

		public override bool Equals(object obj)
		{
			var validator = obj as PersonAccountBalanceValidator;
			return validator != null;
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int result = (GetType().GetHashCode());
				result = (result*397) ^
						 (BudgetGroupHeadCountSpecification != null ? BudgetGroupHeadCountSpecification.GetHashCode() : 0);
				return result;
			}
		}

		private static bool waitlistingIsEnabled(IAbsenceRequest absenceRequest)
		{
			var person = absenceRequest.Person;
			var workflowControlSet = person.WorkflowControlSet;
			if (workflowControlSet != null && workflowControlSet.WaitlistingIsEnabled(absenceRequest))
			{
				return true;
			}
			return false;
		}
	}
}