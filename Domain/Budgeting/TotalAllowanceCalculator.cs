using System;
using System.Collections.Generic;
using System.Globalization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Budgeting
{
    public class TotalAllowanceCalculator : ICalculator
    {
        private readonly INetStaffCalculator _netStaffCalculator;
        private readonly CultureInfo _cultureInfo;

        public TotalAllowanceCalculator(INetStaffCalculator netStaffCalculator, CultureInfo cultureInfo)
        {
            _netStaffCalculator = netStaffCalculator;
            _cultureInfo = cultureInfo;
        }

        public void Calculate(IBudgetDay budgetDay, IEnumerable<IBudgetDay> budgetDayList, BudgetCalculationResult budgetCalculationResult)
        {
            new BudgetedLeaveCalculator(_netStaffCalculator).Calculate(budgetDay, budgetDayList, budgetCalculationResult);
            new BudgetedSurplusCalculator(_netStaffCalculator, _cultureInfo).Calculate(budgetDay, budgetDayList, budgetCalculationResult);
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