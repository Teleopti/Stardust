﻿using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Budgeting
{
	public class AllowanceCalculator : ICalculator
	{
		public void Calculate(IBudgetDay budgetDay, IEnumerable<IBudgetDay> budgetDayList,
			ref BudgetCalculationResult budgetCalculationResult)
		{
			budgetCalculationResult.ShrinkedAllowance = budgetDay.AbsenceThreshold.Value * budgetCalculationResult.FullAllowance;
		}
	}
}