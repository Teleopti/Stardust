using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Domain.WorkflowControl
{
	public class PersonAccountBalanceValidator : IAbsenceRequestValidator
	{
		//TODO: What is this??
		public IBudgetGroupHeadCountSpecification BudgetGroupHeadCountSpecification { get; set; }

		public string InvalidReason => nameof(Resources.RequestDenyReasonPersonAccount);

		public string DisplayText => Resources.Yes;

		public IValidatedRequest Validate(IAbsenceRequest absenceRequest,
			RequiredForHandlingAbsenceRequest requiredForHandlingAbsenceRequest)
		{
			InParameter.NotNull(nameof(requiredForHandlingAbsenceRequest.SchedulingResultStateHolder), requiredForHandlingAbsenceRequest.SchedulingResultStateHolder);
			InParameter.NotNull(nameof(requiredForHandlingAbsenceRequest.PersonAccountBalanceCalculator),
				requiredForHandlingAbsenceRequest.PersonAccountBalanceCalculator);

			var person = absenceRequest.Person;

			var validatedRequest = new ValidatedRequest
			{
				IsValid = requiredForHandlingAbsenceRequest.PersonAccountBalanceCalculator.CheckBalance(
					requiredForHandlingAbsenceRequest.SchedulingResultStateHolder.Schedules[person],
					absenceRequest.Period.ToDateOnlyPeriod(person.PermissionInformation.DefaultTimeZone()))
			};

			if (!validatedRequest.IsValid)
			{
				validatedRequest.ValidationErrors =
					UserTexts.Resources.ResourceManager.GetString(InvalidReason,
						person.PermissionInformation.UICulture());
				validatedRequest.DenyOption = PersonRequestDenyOption.InsufficientPersonAccount;
			}
			return validatedRequest;
		}

		public IAbsenceRequestValidator CreateInstance()
		{
			return new PersonAccountBalanceValidator();
		}

		public override bool Equals(object obj)
		{
			return obj is PersonAccountBalanceValidator;
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int result = GetType().GetHashCode();
				result = (result*397) ^
						 (BudgetGroupHeadCountSpecification?.GetHashCode() ?? 0);
				return result;
			}
		}
	}
}