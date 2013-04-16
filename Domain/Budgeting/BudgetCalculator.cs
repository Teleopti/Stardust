using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Budgeting
{
    public class BudgetCalculator : IBudgetCalculator
    {
        private readonly IEnumerable<IBudgetDay> _budgetDayList;
        private readonly INetStaffCalculator _netStaffCalculator;
        private IList<ICalculator> _calculatorList;

        public BudgetCalculator(IEnumerable<IBudgetDay> listOfBudgetDays, INetStaffCalculator netStaffCalculator, IList<ICalculator> listOfCalculators)
        {
            _budgetDayList = listOfBudgetDays;
            _netStaffCalculator = netStaffCalculator;
            _calculatorList = listOfCalculators;
            _netStaffCalculator.Initialize(_budgetDayList);
        }

	    public IList<ICalculator> CalculatorList
	    {
			get { return _calculatorList; }
			set { _calculatorList = value; }
	    }

        public BudgetCalculationResult Calculate(IBudgetDay budgetDay)
        {
            InParameter.NotNull("budgetDay", budgetDay);
            if(budgetDay.IsClosed) return new BudgetCalculationResult();
            var budgetDayCalculations = _netStaffCalculator.CalculatedResult(budgetDay);
	        foreach (var calculator in _calculatorList)
		        calculator.Calculate(budgetDay, _budgetDayList, ref budgetDayCalculations);

	        return budgetDayCalculations;
        }
    }
}
