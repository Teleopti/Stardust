using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Budgeting
{
    public class BudgetedSurplusCalculator : ICalculator
    {
        private readonly INetStaffCalculator _netStaffCalculator;
        private readonly CultureInfo _cultureInfo;

        public BudgetedSurplusCalculator(INetStaffCalculator netStaffCalculator, CultureInfo cultureInfo)
        {
            _netStaffCalculator = netStaffCalculator;
            _cultureInfo = cultureInfo;
        }

        public void Calculate(IBudgetDay budgetDay, IEnumerable<IBudgetDay> budgetDayList, BudgetCalculationResult budgetCalculationResult)
        {
            var difference = new DifferenceCalculator(_netStaffCalculator, _cultureInfo);
            difference.Calculate(budgetDay, budgetDayList, budgetCalculationResult);
            
            var totalEfficiencyShrinkages = 0d;
            var shrinkages = budgetDay.BudgetGroup.CustomEfficiencyShrinkages;
            totalEfficiencyShrinkages += shrinkages.Where(shrinkage => shrinkage.Id != null).Sum(shrinkage => budgetDay.CustomEfficiencyShrinkages.GetEfficiencyShrinkage(shrinkage.Id.Value).Value);
            
            if (totalEfficiencyShrinkages == 0d)
            {
                budgetCalculationResult.BudgetedSurplus = budgetCalculationResult.Difference;
                return;
            }
            budgetCalculationResult.BudgetedSurplus = budgetCalculationResult.Difference/(1 - totalEfficiencyShrinkages);
        }
    }
}
