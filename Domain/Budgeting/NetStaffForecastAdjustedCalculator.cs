using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Budgeting
{
    public class NetStaffForecastAdjustCalculator : ICalculator
    {
        private readonly INetStaffCalculator _netStaffCalculator;
        private readonly CultureInfo _cultureInfo;

        public NetStaffForecastAdjustCalculator(INetStaffCalculator netStaffCalculator, CultureInfo cultureInfo)
        {
            _netStaffCalculator = netStaffCalculator;
            _cultureInfo = cultureInfo;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2")]
        public void Calculate(IBudgetDay budgetDay, IEnumerable<IBudgetDay> budgetDayList, BudgetCalculationResult budgetCalculationResult)
        {
            var week = DateHelper.GetWeekPeriod(budgetDay.Day, _cultureInfo);

            var budgetDaysWithinWeek = new List<IBudgetDay>(7);
            foreach (IBudgetDay day in budgetDayList)
            {
                if (day.Day > week.EndDate) break;
                if (day.Day < week.StartDate) continue;

                budgetDaysWithinWeek.Add(day);
            }

            var currentNetStaffWeekSum = GetWeekSumNetStaff(budgetDaysWithinWeek);
            var currentForecastedHoursWeekSum = GetWeekSumForecastedHours(budgetDaysWithinWeek);

            double netStaffFcAdj = 0;
            if (currentForecastedHoursWeekSum != 0)
            {
                netStaffFcAdj = (budgetDay.ForecastedHours/currentForecastedHoursWeekSum) * currentNetStaffWeekSum;
            }

            budgetCalculationResult.NetStaffFcAdj = netStaffFcAdj;
        }

        private double GetWeekSumNetStaff(IEnumerable<IBudgetDay> weekBudgetDays)
        {
            double weekNetStaff = 0;
            foreach (var budgetDay in weekBudgetDays)
            {
                var calculatedResult = _netStaffCalculator.CalculatedResult(budgetDay);
                weekNetStaff += calculatedResult.NetStaff;
            }
            return weekNetStaff;
        }

        private static double GetWeekSumForecastedHours(IEnumerable<IBudgetDay> weekBudgetDays)
        {
            double weekForecastedHours = 0;
            foreach (var budgetDay in weekBudgetDays)
            {
                weekForecastedHours += budgetDay.ForecastedHours;
            }
            return weekForecastedHours;
        }
    }
}
