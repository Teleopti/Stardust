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
		public RequiredForHandlingAbsenceRequest(ISchedulingResultStateHolder schedulingResultStateHolder, IPersonAccountBalanceCalculator personAccountBalanceCalculator, IResourceOptimization resourceOptimizationHelper, IBudgetGroupAllowanceSpecification budgetGroupAllowanceSpecification, IBudgetGroupHeadCountSpecification budgetGroupHeadCountSpecification = null)
		{
			ResourceOptimizationHelper = resourceOptimizationHelper;
			BudgetGroupAllowanceSpecification = budgetGroupAllowanceSpecification;
			BudgetGroupHeadCountSpecification = budgetGroupHeadCountSpecification;
			SchedulingResultStateHolder = schedulingResultStateHolder;
			PersonAccountBalanceCalculator = personAccountBalanceCalculator;
		}

		public ISchedulingResultStateHolder SchedulingResultStateHolder { get; }

		public IPersonAccountBalanceCalculator PersonAccountBalanceCalculator { get; }

		public IBudgetGroupAllowanceSpecification BudgetGroupAllowanceSpecification { get; }

		public IResourceOptimization ResourceOptimizationHelper { get; }

		public IBudgetGroupHeadCountSpecification BudgetGroupHeadCountSpecification { get; }
	}
}