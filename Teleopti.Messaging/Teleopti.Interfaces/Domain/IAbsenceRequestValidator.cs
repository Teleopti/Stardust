namespace Teleopti.Interfaces.Domain
{
    public interface IAbsenceRequestValidator
    {

        string InvalidReason { get; }

        string DisplayText { get; }


        IValidatedRequest Validate(IAbsenceRequest absenceRequest, RequiredForHandlingAbsenceRequest requiredForHandlingAbsenceRequest);

        IAbsenceRequestValidator CreateInstance();
    }

    public struct RequiredForHandlingAbsenceRequest
    {
        private readonly ISchedulingResultStateHolder _schedulingResultStateHolder;
        private readonly IPersonAccountBalanceCalculator _personAccountBalanceCalculator;
        private readonly IBudgetGroupAllowanceSpecification _budgetGroupAllowanceSpecification;
        private readonly IBudgetGroupHeadCountSpecification _budgetGroupHeadCountSpecification;
        private readonly IResourceOptimizationHelper _resourceOptimizationHelper;

        public RequiredForHandlingAbsenceRequest(ISchedulingResultStateHolder schedulingResultStateHolder, IPersonAccountBalanceCalculator personAccountBalanceCalculator, IResourceOptimizationHelper resourceOptimizationHelper, IBudgetGroupAllowanceSpecification budgetGroupAllowanceSpecification, IBudgetGroupHeadCountSpecification budgetGroupHeadCountSpecification = null)
        {
            _resourceOptimizationHelper = resourceOptimizationHelper;
            _budgetGroupAllowanceSpecification = budgetGroupAllowanceSpecification;
            _budgetGroupHeadCountSpecification = budgetGroupHeadCountSpecification;
            _schedulingResultStateHolder = schedulingResultStateHolder;
            _personAccountBalanceCalculator = personAccountBalanceCalculator;
        }

        public ISchedulingResultStateHolder SchedulingResultStateHolder
        {
            get { return _schedulingResultStateHolder; }
        }

        public IPersonAccountBalanceCalculator PersonAccountBalanceCalculator
        {
            get { return _personAccountBalanceCalculator; }
        }

        public IBudgetGroupAllowanceSpecification BudgetGroupAllowanceSpecification
        {
            get { return _budgetGroupAllowanceSpecification; }
        }

        public IResourceOptimizationHelper ResourceOptimizationHelper
        {
            get { return _resourceOptimizationHelper; }
        }

        public IBudgetGroupHeadCountSpecification BudgetGroupHeadCountSpecification
        {
            get { return _budgetGroupHeadCountSpecification; }
        }
    }
}