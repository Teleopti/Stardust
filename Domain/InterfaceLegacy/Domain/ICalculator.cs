using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
	/// <summary>
	/// Budget calculator interface
	/// </summary>
	public interface ICalculator
	{
		/// <summary>
		/// Caculate method
		/// </summary>
		/// <param name="budgetDay">Current budgetday to calculate</param>
		/// <param name="budgetDayList">List of budgetdays that are opened in budget</param>
		/// <param name="budgetCalculationResult">Class that holds the results</param>
		void Calculate(IBudgetDay budgetDay, IEnumerable<IBudgetDay> budgetDayList, ref BudgetCalculationResult budgetCalculationResult);
	}
}
