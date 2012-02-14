using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.WorkflowControl
{
    public class PersonAccountBalanceValidator : IAbsenceRequestValidator
    {
        public ISchedulingResultStateHolder SchedulingResultStateHolder { get; set; }
        public IPersonAccountBalanceCalculator PersonAccountBalanceCalculator { get; set; }
        public IResourceOptimizationHelper ResourceOptimizationHelper { get; set; }
        public IBudgetGroupAllowanceSpecification BudgetGroupAllowanceSpecification { get; set; }

        public string InvalidReason
        {
            get { return "RequestDenyReasonPersonAccount"; }
        }

        public string DisplayText
        {
            get { return UserTexts.Resources.Yes; }
        }

        public bool Validate(IAbsenceRequest absenceRequest)
        {
            InParameter.NotNull("SchedulingResultStateHolder", SchedulingResultStateHolder);
            InParameter.NotNull("PersonAccountBalanceCalculator", PersonAccountBalanceCalculator);

            var person = absenceRequest.Person;
            return
                PersonAccountBalanceCalculator.CheckBalance(SchedulingResultStateHolder.Schedules[person],
                                                            absenceRequest.Period.ToDateOnlyPeriod(
                                                                person.PermissionInformation.DefaultTimeZone()));
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
                result = (result * 397) ^ (GetType().GetHashCode());
                return result;
            }
        }
    }
}