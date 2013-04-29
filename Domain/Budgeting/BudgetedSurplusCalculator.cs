using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Budgeting
{
    public class BudgetedSurplusCalculator : ICalculator
    {
	    public void Calculate(IBudgetDay budgetDay, IEnumerable<IBudgetDay> budgetDayList, ref BudgetCalculationResult budgetCalculationResult)
        {
            var difference = new DifferenceCalculator();
            difference.Calculate(budgetDay, budgetDayList, ref budgetCalculationResult);
            
            var totalEfficiencyShrinkages = 0d;
            var shrinkages = budgetDay.BudgetGroup.CustomEfficiencyShrinkages;
            totalEfficiencyShrinkages += shrinkages.Where(shrinkage => shrinkage.Id != null).Sum(shrinkage => budgetDay.CustomEfficiencyShrinkages.GetEfficiencyShrinkage(shrinkage.Id.Value).Value);
            
            if (totalEfficiencyShrinkages.Equals(0))
            {
                budgetCalculationResult.BudgetedSurplus = budgetCalculationResult.Difference;
                return;
            }
            budgetCalculationResult.BudgetedSurplus = budgetCalculationResult.Difference/(1 - totalEfficiencyShrinkages);
        }
    }
}
