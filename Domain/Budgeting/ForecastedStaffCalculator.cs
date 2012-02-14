using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Budgeting
{
    public class ForecastedStaffCalculator : ICalculator
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2")]
        public void Calculate(IBudgetDay budgetDay, IEnumerable<IBudgetDay> budgetDayList, BudgetCalculationResult budgetCalculationResult)
        {
            double forecastedStaff = 0;
            if (budgetDay.FulltimeEquivalentHours != 0)
            {
                forecastedStaff =  budgetDay.ForecastedHours/budgetDay.FulltimeEquivalentHours;
            }
            budgetCalculationResult.ForecastedStaff = forecastedStaff;
        }
    }
}
