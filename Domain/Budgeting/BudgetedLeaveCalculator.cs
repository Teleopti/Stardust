﻿using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Budgeting
{
    public class BudgetedLeaveCalculator : ICalculator
    {
        private readonly INetStaffCalculator _netStaffCalculator;

        public BudgetedLeaveCalculator(INetStaffCalculator netStaffCalculator)
        {
            _netStaffCalculator = netStaffCalculator;
        }

        public void Calculate(IBudgetDay budgetDay, IEnumerable<IBudgetDay> budgetDayList, ref BudgetCalculationResult budgetCalculationResult)
        {
            if (budgetDay.IsClosed)
            {
                budgetCalculationResult.BudgetedLeave = 0;
                return;
            }
            var totalShrinkages = 0d;
            var shrinkages = budgetDay.BudgetGroup.CustomShrinkages;
            totalShrinkages += shrinkages.Where(shrinkage => shrinkage.Id != null && shrinkage.IncludedInAllowance).Sum(shrinkage => budgetDay.CustomShrinkages.GetShrinkage(shrinkage.Id.Value).Value);
            
            if (!budgetDay.FulltimeEquivalentHours.Equals(0))
            {
                budgetCalculationResult = _netStaffCalculator.CalculatedResult(budgetDay);
                var totalFtEs = budgetCalculationResult.GrossStaff +
                                budgetDay.Contractors / budgetDay.FulltimeEquivalentHours;
                budgetCalculationResult.BudgetedLeave = totalFtEs * totalShrinkages;
            }
        }
    }
}
