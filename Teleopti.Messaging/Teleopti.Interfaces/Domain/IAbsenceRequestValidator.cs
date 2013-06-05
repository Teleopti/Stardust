namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Base Interface for Absence Request validators.
    /// </summary>
    /// <remarks>
    /// Created by: HenryG
    /// Created date: 2010-04-19
    /// </remarks>
    public interface IAbsenceRequestValidator
    {
        /// <summary>
        /// Gets the invalid reason text.
        /// </summary>
        /// <value>The invalid reason.</value>
        /// <remarks>
        /// Created by: HenryG
        /// Created date: 2010-04-19
        /// </remarks>
        string InvalidReason { get; }

        /// <summary>
        /// Gets the display text.
        /// </summary>
        /// <value>The display text.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2010-04-27
        /// </remarks>
        string DisplayText { get; }

        /// <summary>
        /// Validates the specified absence request.
        /// </summary>
        /// <param name="absenceRequest">The absence request.</param>
        /// <param name="requiredForHandlingAbsenceRequest">The required state holders for approving an absence request.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: HenryG
        /// Created date: 2010-04-19
        /// </remarks>
        IValidatedRequest Validate(IAbsenceRequest absenceRequest, RequiredForHandlingAbsenceRequest requiredForHandlingAbsenceRequest);

        /// <summary>
        /// Creates a new instance of the same validator type. To avoid threading issues.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2010-04-19
        /// </remarks>
        IAbsenceRequestValidator CreateInstance();
    }

    /// <summary>
    /// 
    /// </summary>
    public struct RequiredForHandlingAbsenceRequest
    {
        private readonly ISchedulingResultStateHolder _schedulingResultStateHolder;
        private readonly IPersonAccountBalanceCalculator _personAccountBalanceCalculator;
        private readonly IBudgetGroupAllowanceSpecification _budgetGroupAllowanceSpecification;
        private readonly IBudgetGroupAllowanceCalculator _budgetGroupAllowanceCalculator;
        private readonly IBudgetGroupHeadCountSpecification _budgetGroupHeadCountSpecification;
        private readonly IResourceOptimizationHelper _resourceOptimizationHelper;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="schedulingResultStateHolder"></param>
        /// <param name="personAccountBalanceCalculator"></param>
        /// <param name="resourceOptimizationHelper"></param>
        /// <param name="budgetGroupAllowanceSpecification"></param>
        /// <param name="budgetGroupAllowanceCalculator"></param>
        /// <param name="budgetGroupHeadCountSpecification"></param>
        public RequiredForHandlingAbsenceRequest(ISchedulingResultStateHolder schedulingResultStateHolder, IPersonAccountBalanceCalculator personAccountBalanceCalculator, IResourceOptimizationHelper resourceOptimizationHelper, IBudgetGroupAllowanceSpecification budgetGroupAllowanceSpecification, IBudgetGroupAllowanceCalculator budgetGroupAllowanceCalculator, IBudgetGroupHeadCountSpecification budgetGroupHeadCountSpecification = null)
        {
            _resourceOptimizationHelper = resourceOptimizationHelper;
            _budgetGroupAllowanceSpecification = budgetGroupAllowanceSpecification;
            _budgetGroupAllowanceCalculator = budgetGroupAllowanceCalculator;
            _budgetGroupHeadCountSpecification = budgetGroupHeadCountSpecification;
            _schedulingResultStateHolder = schedulingResultStateHolder;
            _personAccountBalanceCalculator = personAccountBalanceCalculator;
        }

        /// <summary>
        /// 
        /// </summary>
        public ISchedulingResultStateHolder SchedulingResultStateHolder
        {
            get { return _schedulingResultStateHolder; }
        }

        /// <summary>
        /// 
        /// </summary>
        public IPersonAccountBalanceCalculator PersonAccountBalanceCalculator
        {
            get { return _personAccountBalanceCalculator; }
        }

        /// <summary>
        /// 
        /// </summary>
        public IBudgetGroupAllowanceSpecification BudgetGroupAllowanceSpecification
        {
            get { return _budgetGroupAllowanceSpecification; }
        }

        /// <summary>
        /// 
        /// </summary>
        public IBudgetGroupAllowanceCalculator BudgetGroupAllowanceCalculator
        {
            get { return _budgetGroupAllowanceCalculator; }
        }

        /// <summary>
        /// 
        /// </summary>
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