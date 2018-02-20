using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface IAbsenceRequestValidator
	{
		string DisplayText { get; }

		IValidatedRequest Validate(IAbsenceRequest absenceRequest, RequiredForHandlingAbsenceRequest requiredForHandlingAbsenceRequest);

		IAbsenceRequestValidator CreateInstance();
	}

	public struct RequiredForHandlingAbsenceRequest
	{
		public RequiredForHandlingAbsenceRequest(ISchedulingResultStateHolder schedulingResultStateHolder, IPersonAccountBalanceCalculator personAccountBalanceCalculator, IResourceCalculation resourceOptimizationHelper, IBudgetGroupAllowanceSpecification budgetGroupAllowanceSpecification, IBudgetGroupHeadCountSpecification budgetGroupHeadCountSpecification = null)
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

		public IResourceCalculation ResourceOptimizationHelper { get; }

		public IBudgetGroupHeadCountSpecification BudgetGroupHeadCountSpecification { get; }
	}
}