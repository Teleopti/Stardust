using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.WorkflowControl
{
    public class BudgetGroupHeadCountValidator : IAbsenceRequestValidator
    {
        public ISchedulingResultStateHolder SchedulingResultStateHolder { get; set; }
        public IPersonAccountBalanceCalculator PersonAccountBalanceCalculator { get; set; }

        public string InvalidReason
        {
            get { return "RequestDenyReasonBudgetGroupAllowance"; }
        }
        
        public IResourceOptimizationHelper ResourceOptimizationHelper { get; set; }


        public string DisplayText
        {
            get { return "BG head count"; }
        }


        public IValidatedRequest Validate(IAbsenceRequest absenceRequest)
        {
            return new ValidatedRequest() {IsValid = true, ValidationErrors = string.Empty};
        }

        public IAbsenceRequestValidator CreateInstance()
        {
            return new BudgetGroupHeadCountValidator();
        }

        
        public IBudgetGroupAllowanceSpecification BudgetGroupAllowanceSpecification { get; set; }
        public IBudgetGroupAllowanceCalculator BudgetGroupAllowanceCalculator { get; set; }

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
                result = (result * 397) ^ (GetType().GetHashCode());
                return result;
            }
        }
    }
}
