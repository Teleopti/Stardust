using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.WorkflowControl
{
    public class AbsenceRequestNoneValidator : IAbsenceRequestValidator
    {
        public ISchedulingResultStateHolder SchedulingResultStateHolder { get; set; }
        public IPersonAccountBalanceCalculator PersonAccountBalanceCalculator { get; set; }
        public IResourceOptimizationHelper ResourceOptimizationHelper { get; set; }
        public IBudgetGroupAllowanceSpecification BudgetGroupAllowanceSpecification { get; set; }
        public IBudgetGroupAllowanceCalculator BudgetGroupAllowanceCalculator { get; set; }
        public IBudgetGroupHeadCountSpecification BudgetGroupHeadCountSpecification { get; set; }

        public string InvalidReason
        {
            get { return string.Empty; }
        }

        public string DisplayText
        {
            get { return UserTexts.Resources.No; }
        }

        public IValidatedRequest Validate(IAbsenceRequest absenceRequest)
        {
            return new ValidatedRequest()
                {
                    IsValid = true,
                    ValidationErrors = ""
                };
        }

        public IAbsenceRequestValidator CreateInstance()
        {
            return new AbsenceRequestNoneValidator();
        }

        public override bool Equals(object obj)
        {
            var validator = obj as AbsenceRequestNoneValidator;
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