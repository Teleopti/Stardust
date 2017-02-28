﻿using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Budgeting
{
    public class ForecastedStaffCalculator : ICalculator
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2")]
        public void Calculate(IBudgetDay budgetDay, IEnumerable<IBudgetDay> budgetDayList, ref BudgetCalculationResult budgetCalculationResult)
        {
            double forecastedStaff = 0;
	        if (!budgetDay.FulltimeEquivalentHours.Equals(0))
		        forecastedStaff = budgetDay.ForecastedHours/budgetDay.FulltimeEquivalentHours;
	        budgetCalculationResult.ForecastedStaff = forecastedStaff;
        }
    }
}
