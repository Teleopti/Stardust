using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.WorkflowControl
{
    public class BudgetGroupAllowanceValidator : IAbsenceRequestValidator
    {
        public ISchedulingResultStateHolder SchedulingResultStateHolder { get; set; }
        public IPersonAccountBalanceCalculator PersonAccountBalanceCalculator { get; set; }
        public IResourceOptimizationHelper ResourceOptimizationHelper { get; set; }
        public IBudgetGroupAllowanceSpecification BudgetGroupAllowanceSpecification { get; set; }
        public IBudgetGroupAllowanceCalculator BudgetGroupAllowanceCalculator { get; set; }

        public string InvalidReason
        {
            get { return "RequestDenyReasonBudgetGroupAllowance"; }
        }

        public string DisplayText
        {
            get { return UserTexts.Resources.BudgetGroup; }
        }

        public IValidatedRequest Validate(IAbsenceRequest absenceRequest)
        {
            var validatedRequest = new ValidatedRequest();
            validatedRequest.IsValid = BudgetGroupAllowanceSpecification.IsSatisfiedBy(absenceRequest);
            validatedRequest.ValidationErrors = string.Empty;

            if(!validatedRequest.IsValid)
                validatedRequest.ValidationErrors = BudgetGroupAllowanceCalculator.CheckBudgetGroup(absenceRequest);

            return validatedRequest;
        }

        public IAbsenceRequestValidator CreateInstance()
        {
            return new BudgetGroupAllowanceValidator();
        }

        public override bool Equals(object obj)
        {
            var validator = obj as BudgetGroupAllowanceValidator;
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
                result = (result * 397) ^ (GetType().GetHashCode());
                return result;
            }
        }
    }
}