using System.Collections.Generic;
using System.Globalization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Budgeting
{
    public class BudgetedStaffCalculator : ICalculator
    {
        private readonly INetStaffCalculator _netStaffCalculator;
        private readonly CultureInfo _cultureInfo;

        public BudgetedStaffCalculator(INetStaffCalculator netStaffCalculator, CultureInfo cultureInfo)
        {
            _netStaffCalculator = netStaffCalculator;
            _cultureInfo = cultureInfo;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2")]
        public void Calculate(IBudgetDay budgetDay, IEnumerable<IBudgetDay> budgetDayList, BudgetCalculationResult budgetCalculationResult) 
        {
            double budgetedStaff = 0;

            var netStaffFcAdjPerDay = new NetStaffForecastAdjustCalculator(_netStaffCalculator, _cultureInfo);
            netStaffFcAdjPerDay.Calculate(budgetDay, budgetDayList, budgetCalculationResult);

            var changeFactor = budgetDay.CustomEfficiencyShrinkages.GetTotal().Value;

            if (budgetDay.FulltimeEquivalentHours != 0)
            {
                budgetedStaff = (budgetCalculationResult.NetStaffFcAdj + (budgetDay.OvertimeHours + budgetDay.StudentHours) / budgetDay.FulltimeEquivalentHours) * (1 - changeFactor);
            }

            budgetCalculationResult.BudgetedStaff = budgetedStaff;
        }
    }
}
