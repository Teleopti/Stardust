using System.Collections.Generic;
using System.Globalization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Budgeting
{
    public class DifferenceCalculator : ICalculator
    {
        private readonly INetStaffCalculator _netStaffCalculator;
        private readonly CultureInfo _cultureInfo;

        public DifferenceCalculator(INetStaffCalculator netStaffCalculator, CultureInfo cultureInfo)
        {
            _netStaffCalculator = netStaffCalculator;
            _cultureInfo = cultureInfo;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2")]
        public void Calculate(IBudgetDay budgetDay, IEnumerable<IBudgetDay> budgetDayList, BudgetCalculationResult budgetCalculationResult)
        {
            var budgetedStaff = new BudgetedStaffCalculator(_netStaffCalculator, _cultureInfo);
            budgetedStaff.Calculate(budgetDay, budgetDayList, budgetCalculationResult);

            var forecastedStaff = new ForecastedStaffCalculator();
            forecastedStaff.Calculate(budgetDay, budgetDayList, budgetCalculationResult);

            budgetCalculationResult.Difference = budgetCalculationResult.BudgetedStaff - budgetCalculationResult.ForecastedStaff;
        }
    }
}
