using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.WorkflowControl
{
    public class PersonAccountBalanceValidator : IAbsenceRequestValidator
    {
        public ISchedulingResultStateHolder SchedulingResultStateHolder { get; set; }
        public IPersonAccountBalanceCalculator PersonAccountBalanceCalculator { get; set; }
        public IResourceOptimizationHelper ResourceOptimizationHelper { get; set; }
        public IBudgetGroupAllowanceSpecification BudgetGroupAllowanceSpecification { get; set; }
        public IBudgetGroupAllowanceCalculator BudgetGroupAllowanceCalculator { get; set; }
        public IBudgetGroupHeadCountSpecification BudgetGroupHeadCountSpecification { get; set; }

        public string InvalidReason
        {
            get { return "RequestDenyReasonPersonAccount"; }
        }

        public string DisplayText
        {
            get { return UserTexts.Resources.Yes; }
        }

        public IValidatedRequest Validate(IAbsenceRequest absenceRequest)
        {
            InParameter.NotNull("SchedulingResultStateHolder", SchedulingResultStateHolder);
            InParameter.NotNull("PersonAccountBalanceCalculator", PersonAccountBalanceCalculator);

            var person = absenceRequest.Person;

            var validatedRequest = new ValidatedRequest();
            validatedRequest.IsValid = PersonAccountBalanceCalculator.CheckBalance(SchedulingResultStateHolder.Schedules[person],
                                                            absenceRequest.Period.ToDateOnlyPeriod(
                                                                person.PermissionInformation.DefaultTimeZone()));

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
                int result = (SchedulingResultStateHolder != null ? SchedulingResultStateHolder.GetHashCode() : 0);
                result = (result * 397) ^ (PersonAccountBalanceCalculator != null ? PersonAccountBalanceCalculator.GetHashCode() : 0);
                result = (result * 397) ^ (ResourceOptimizationHelper != null ? ResourceOptimizationHelper.GetHashCode() : 0);
                result = (result * 397) ^ (BudgetGroupAllowanceSpecification != null ? BudgetGroupAllowanceSpecification.GetHashCode() : 0);
                result = (result * 397) ^ (BudgetGroupAllowanceCalculator != null ? BudgetGroupAllowanceCalculator.GetHashCode() : 0);
                result = (result * 397) ^ (BudgetGroupHeadCountSpecification != null ? BudgetGroupHeadCountSpecification.GetHashCode() : 0);
                result = (result * 397) ^ (GetType().GetHashCode());
                return result;
            }
        }
    }
}