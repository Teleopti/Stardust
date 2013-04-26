using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Budgeting
{
    public class BudgetedStaffCalculator : ICalculator
    {
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2")]
        public void Calculate(IBudgetDay budgetDay, IEnumerable<IBudgetDay> budgetDayList, ref BudgetCalculationResult budgetCalculationResult) 
        {
            double budgetedStaff = 0;
			var changeFactor = budgetDay.CustomEfficiencyShrinkages.GetTotal().Value;

	        if (!budgetDay.FulltimeEquivalentHours.Equals(0))
		        budgetedStaff = (budgetCalculationResult.NetStaffFcAdj +
		                         (budgetDay.OvertimeHours + budgetDay.StudentHours)/budgetDay.FulltimeEquivalentHours)*
		                        (1 - changeFactor);

	        budgetCalculationResult.BudgetedStaff = budgetedStaff;
        }
    }
}
