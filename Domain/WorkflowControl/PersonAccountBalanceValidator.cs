using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.WorkflowControl
{
    public class PersonAccountBalanceValidator : IAbsenceRequestValidator
    {
        public IBudgetGroupHeadCountSpecification BudgetGroupHeadCountSpecification { get; set; }
        public string InvalidReason
        {
            get { return "RequestDenyReasonPersonAccount"; }
        }

        public string DisplayText
        {
            get { return UserTexts.Resources.Yes; }
        }

        public IValidatedRequest Validate(IAbsenceRequest absenceRequest, RequiredForHandlingAbsenceRequest requiredForHandlingAbsenceRequest)
        {
            InParameter.NotNull("SchedulingResultStateHolder", requiredForHandlingAbsenceRequest.SchedulingResultStateHolder);
            InParameter.NotNull("PersonAccountBalanceCalculator", requiredForHandlingAbsenceRequest.PersonAccountBalanceCalculator);

            var person = absenceRequest.Person;

            var validatedRequest = new ValidatedRequest();
            validatedRequest.IsValid =
                requiredForHandlingAbsenceRequest.PersonAccountBalanceCalculator.CheckBalance(
                    requiredForHandlingAbsenceRequest.SchedulingResultStateHolder.Schedules[person],
                    absenceRequest.Period.ToDateOnlyPeriod(person.PermissionInformation.DefaultTimeZone()));

            if (!validatedRequest.IsValid)
                validatedRequest.ValidationErrors =
                    UserTexts.Resources.ResourceManager.GetString("RequestDenyReasonPersonAccount",
                                                                  person.PermissionInformation.Culture());
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
                result = (result * 397) ^ (BudgetGroupHeadCountSpecification != null ? BudgetGroupHeadCountSpecification.GetHashCode() : 0);
                return result;
            }
        }
    }
}