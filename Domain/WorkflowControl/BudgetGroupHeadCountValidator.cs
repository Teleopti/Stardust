using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.WorkflowControl
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "HeadCount")]
    public class BudgetGroupHeadCountValidator : IAbsenceRequestValidator
    {
        public string InvalidReason
        {
            get { return "RequestDenyReasonBudgetGroupAllowance"; }
        }
        
        public string DisplayText
        {
            get { return UserTexts.Resources.BudgetGroupHeadCount; }
        }

        public IValidatedRequest Validate(IAbsenceRequest absenceRequest, RequiredForHandlingAbsenceRequest requiredForHandlingAbsenceRequest)
        {
           var validatedRequest = requiredForHandlingAbsenceRequest.BudgetGroupHeadCountSpecification.IsSatisfied(absenceRequest);
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
                int result = (GetType().GetHashCode());
                return result;
            }
        }
    }
}
