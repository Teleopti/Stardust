using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.WorkflowControl
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "HeadCount")]
    public class BudgetGroupHeadCountValidator : IAbsenceRequestValidator
    {
        public ISchedulingResultStateHolder SchedulingResultStateHolder { get; set; }
        public IPersonAccountBalanceCalculator PersonAccountBalanceCalculator { get; set; }
        public IBudgetGroupAllowanceSpecification BudgetGroupAllowanceSpecification { get; set; }
        public IBudgetGroupAllowanceCalculator BudgetGroupAllowanceCalculator { get; set; }
        public IBudgetGroupHeadCountSpecification BudgetGroupHeadCountSpecification { get; set; }

        public string InvalidReason
        {
            get { return "RequestDenyReasonBudgetGroupAllowance"; }
        }
        
        public IResourceOptimizationHelper ResourceOptimizationHelper { get; set; }


        public string DisplayText
        {
            get { return UserTexts.Resources.BudgetGroupHeadCount; }
        }


        public IValidatedRequest Validate(IAbsenceRequest absenceRequest)
        {
            var validatedRequest = new ValidatedRequest();
            validatedRequest.IsValid = BudgetGroupHeadCountSpecification.IsSatisfiedBy(absenceRequest);
            validatedRequest.ValidationErrors = string.Empty;

            if (!validatedRequest.IsValid)
                validatedRequest.ValidationErrors = BudgetGroupAllowanceCalculator.CheckHeadCountInBudgetGroup(absenceRequest);

            return validatedRequest;
        }

        public IAbsenceRequestValidator CreateInstance()
        {
            return new BudgetGroupHeadCountValidator();
        }

        public override bool Equals(object obj)
        {
            var validator = obj as BudgetGroupHeadCountValidator;
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
