﻿using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Budgeting
{
    public class DifferencePercentCalculator : ICalculator
    {
	    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2")]
        public void Calculate(IBudgetDay budgetDay, IEnumerable<IBudgetDay> budgetDayList, ref BudgetCalculationResult budgetCalculationResult) // Get latest staffemployed and daysperyear
        {
            double differencePercent = 0;
            var forecastedStaff = new ForecastedStaffCalculator();
            forecastedStaff.Calculate(budgetDay, budgetDayList, ref budgetCalculationResult);

		    if (!budgetCalculationResult.ForecastedStaff.Equals(0))
			    differencePercent = budgetCalculationResult.Difference/budgetCalculationResult.ForecastedStaff;

		    budgetCalculationResult.DifferencePercent = new Percent(differencePercent); 
        }
    }
}
