using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Specification;

namespace Teleopti.Ccc.Domain.Budgeting
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "HeadCount")]
	public class BudgetGroupHeadCountSpecification : PersonRequestSpecification<IAbsenceRequestAndSchedules>, IBudgetGroupHeadCountSpecification
	{
		private readonly IBudgetGroupHeadCountCalculator _budgetGroupHeadCountCalculator;

		public BudgetGroupHeadCountSpecification(IBudgetGroupHeadCountCalculator budgetGroupHeadCountCalculator)
		{
			_budgetGroupHeadCountCalculator = budgetGroupHeadCountCalculator;
		}

		public override IValidatedRequest IsSatisfied(IAbsenceRequestAndSchedules absenceRequest)
		{
			return _budgetGroupHeadCountCalculator.IsSatisfied(absenceRequest);
		}
	}

	public interface IBudgetGroupHeadCountCalculator
	{
		IValidatedRequest IsSatisfied(IAbsenceRequestAndSchedules absenceRequestSchedules);
	}
}