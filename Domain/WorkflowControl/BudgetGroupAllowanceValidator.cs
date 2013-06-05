using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.WorkflowControl
{
    public class BudgetGroupAllowanceValidator : IAbsenceRequestValidator
    {
        public string InvalidReason
        {
            get { return "RequestDenyReasonBudgetGroupAllowance"; }
        }

        public string DisplayText
        {
            get { return UserTexts.Resources.BudgetGroup; }
        }

        public IValidatedRequest Validate(IAbsenceRequest absenceRequest, RequiredForHandlingAbsenceRequest requiredForHandlingAbsenceRequest)
        {
            var validatedRequest = new ValidatedRequest();
            validatedRequest.IsValid = requiredForHandlingAbsenceRequest.BudgetGroupAllowanceSpecification.IsSatisfiedBy(absenceRequest);
            validatedRequest.ValidationErrors = string.Empty;

            if(!validatedRequest.IsValid)
                validatedRequest.ValidationErrors = requiredForHandlingAbsenceRequest.BudgetGroupAllowanceCalculator.CheckBudgetGroup(absenceRequest);

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
                int result = (GetType().GetHashCode());
                return result;
            }
        }
    }
}