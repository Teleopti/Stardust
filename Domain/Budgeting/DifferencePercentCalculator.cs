using System.Collections.Generic;
using System.Globalization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Budgeting
{
    public class DifferencePercentCalculator : ICalculator
    {
        private readonly INetStaffCalculator _netStaffCalculator;
        private readonly CultureInfo _cultureInfo;

        public DifferencePercentCalculator(INetStaffCalculator netStaffCalculator, CultureInfo cultureInfo)
        {
            _netStaffCalculator = netStaffCalculator;
            _cultureInfo = cultureInfo;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2")]
        public void Calculate(IBudgetDay budgetDay, IEnumerable<IBudgetDay> budgetDayList, BudgetCalculationResult budgetCalculationResult) // Get latest staffemployed and daysperyear
        {
            double differencePercent = 0;
            var difference = new DifferenceCalculator(_netStaffCalculator, _cultureInfo);
            difference.Calculate(budgetDay, budgetDayList, budgetCalculationResult);

            var forecastedStaff = new ForecastedStaffCalculator();
            forecastedStaff.Calculate(budgetDay, budgetDayList, budgetCalculationResult);

            if (budgetCalculationResult.ForecastedStaff != 0)
            {
                differencePercent = budgetCalculationResult.Difference / budgetCalculationResult.ForecastedStaff;
            }

            budgetCalculationResult.DifferencePercent = new Percent(differencePercent); 
        }
    }
}
