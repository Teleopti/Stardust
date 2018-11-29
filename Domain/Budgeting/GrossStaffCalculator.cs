using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Budgeting
{
    public class GrossStaffCalculator : IGrossStaffCalculator
    {
        private readonly IDictionary<DateOnly, BudgetCalculationResult> _budgetCalculationResults =
            new Dictionary<DateOnly, BudgetCalculationResult>();

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2")]
        public BudgetCalculationResult CalculatedResult(IBudgetDay budgetDay) // Get latest staffemployed and daysperyear
        {
            return _budgetCalculationResults[budgetDay.Day];
        }

        public void Initialize(IEnumerable<IBudgetDay> budgetDayList)
        {
            var budgetDay = budgetDayList.FirstOrDefault();
            if (budgetDay == null) return;

            var daysPerYear = budgetDay.BudgetGroup.DaysPerYear;
            double previousDayGrossStaff = 0;
            foreach (var day in budgetDayList)
            {
                var staffEmployed = day.StaffEmployed.GetValueOrDefault(previousDayGrossStaff);
                var grossStaffThisDay = staffEmployed*(1 - day.AttritionRate.Value/daysPerYear) + day.Recruitment;
                _budgetCalculationResults.Add(day.Day, new BudgetCalculationResult {GrossStaff = grossStaffThisDay});
                previousDayGrossStaff = grossStaffThisDay;
            }
        }
    }

    public interface IGrossStaffCalculator
    {
        void Initialize(IEnumerable<IBudgetDay> budgetDayList);
        BudgetCalculationResult CalculatedResult(IBudgetDay budgetDay);
    }
}
