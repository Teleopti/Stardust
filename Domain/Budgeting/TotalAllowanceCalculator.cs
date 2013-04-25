using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Budgeting
{
    public class TotalAllowanceCalculator : ICalculator
    {
	    public void Calculate(IBudgetDay budgetDay, IEnumerable<IBudgetDay> budgetDayList, ref BudgetCalculationResult budgetCalculationResult)
        {
            if (Math.Abs(budgetDay.FulltimeEquivalentHours - 0d) < double.Epsilon)
            {
                budgetCalculationResult.TotalAllowance = 0d;
                return;
            }
            if (budgetDay.AbsenceOverride.HasValue)
            {
                budgetCalculationResult.TotalAllowance = budgetDay.AbsenceOverride.Value;
                return;
            }
           
            budgetCalculationResult.TotalAllowance = (budgetCalculationResult.BudgetedLeave + budgetCalculationResult.BudgetedSurplus) + (budgetDay.AbsenceExtra ?? 0d);
        }
    }
}