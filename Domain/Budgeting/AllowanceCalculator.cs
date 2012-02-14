using System.Collections.Generic;
using System.Globalization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Budgeting
{
    public class AllowanceCalculator : ICalculator
    {
        private readonly INetStaffCalculator _netStaffCalculator;
        private readonly CultureInfo _cultureInfo;

        public AllowanceCalculator(INetStaffCalculator netStaffCalculator, CultureInfo cultureInfo)
        {
            _netStaffCalculator = netStaffCalculator;
            _cultureInfo = cultureInfo;
        }

        public void Calculate(IBudgetDay budgetDay, IEnumerable<IBudgetDay> budgetDayList, BudgetCalculationResult budgetCalculationResult)
        {
            var totalAllowanceCalculator = new TotalAllowanceCalculator(_netStaffCalculator, _cultureInfo);
            totalAllowanceCalculator.Calculate(budgetDay,budgetDayList, budgetCalculationResult);

            budgetCalculationResult.Allowance = budgetDay.AbsenceThreshold.Value*budgetCalculationResult.TotalAllowance;
        }
    }
}
