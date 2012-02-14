using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Budgeting
{
    public class NetStaffCalculator : INetStaffCalculator
    {
        private readonly IGrossStaffCalculator _grossStaffCalculator;

        public NetStaffCalculator(IGrossStaffCalculator grossStaffCalculator)
        {
            _grossStaffCalculator = grossStaffCalculator;
        }

        public void Initialize(IEnumerable<IBudgetDay> budgetDayList)
        {
            _grossStaffCalculator.Initialize(budgetDayList);

            const double weekLength = 7;
            foreach (var budgetDay in budgetDayList)
            {
                var budgetCalculationResult = _grossStaffCalculator.CalculatedResult(budgetDay);

                if (budgetDay.IsClosed)
                {
                    budgetCalculationResult.NetStaff = 0;
                    continue;
                }

                budgetCalculationResult.NetStaff = 0;
				if (budgetDay.FulltimeEquivalentHours != 0)
				{
					var changeFactor = budgetDay.CustomShrinkages.GetTotal().Value;
					var netStaff = (budgetCalculationResult.GrossStaff + budgetDay.Contractors/budgetDay.FulltimeEquivalentHours)*
					               (1 - changeFactor)*
					               (weekLength - budgetDay.DaysOffPerWeek)/weekLength;
					budgetCalculationResult.NetStaff = netStaff;
				}
            }
        }

        public BudgetCalculationResult CalculatedResult(IBudgetDay budgetDay)
        {
            return _grossStaffCalculator.CalculatedResult(budgetDay);
        }
    }

    public interface INetStaffCalculator
    {
        void Initialize(IEnumerable<IBudgetDay> budgetDayList);
        BudgetCalculationResult CalculatedResult(IBudgetDay budgetDay);
    }
}
