using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Budgeting
{
	public class AllowanceCalculator : ICalculator
	{
		public void Calculate(IBudgetDay budgetDay, IEnumerable<IBudgetDay> budgetDayList,
			ref BudgetCalculationResult budgetCalculationResult)
		{
			budgetCalculationResult.ShrinkedAllowance = budgetDay.AbsenceThreshold.Value * budgetCalculationResult.FullAllowance;
		}
	}
}