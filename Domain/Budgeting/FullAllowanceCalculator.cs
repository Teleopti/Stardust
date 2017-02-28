using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Budgeting
{
	public class FullAllowanceCalculator : ICalculator
	{
		public void Calculate(IBudgetDay budgetDay, IEnumerable<IBudgetDay> budgetDayList,
			ref BudgetCalculationResult budgetCalculationResult)
		{
			if (Math.Abs(budgetDay.FulltimeEquivalentHours - 0d) < double.Epsilon)
			{
				budgetCalculationResult.FullAllowance = 0d;
				return;
			}
			if (budgetDay.AbsenceOverride.HasValue)
			{
				budgetCalculationResult.FullAllowance = budgetDay.AbsenceOverride.Value;
				return;
			}

			budgetCalculationResult.FullAllowance = budgetCalculationResult.BudgetedLeave +
													budgetCalculationResult.BudgetedSurplus + (budgetDay.AbsenceExtra ?? 0d);
		}
	}
}