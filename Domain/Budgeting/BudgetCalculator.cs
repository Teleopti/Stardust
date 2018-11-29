using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

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
			InParameter.NotNull(nameof(budgetDay), budgetDay);
			var budgetDayCalculations = _netStaffCalculator.CalculatedResult(budgetDay);
			foreach (var calculator in _calculatorList)
				calculator.Calculate(budgetDay, _budgetDayList, ref budgetDayCalculations);

			if (!budgetDay.IsClosed) return budgetDayCalculations;

			return budgetDay.AbsenceOverride == null
				? new BudgetCalculationResult()
				: new BudgetCalculationResult
				{
					ShrinkedAllowance = budgetDayCalculations.ShrinkedAllowance,
					FullAllowance = budgetDayCalculations.FullAllowance
				};
		}

		public BudgetCalculationResult CalculateWithoutNetStaffFcAdj(IBudgetDay budgetDay, double netStaffFcAdj)
		{
			InParameter.NotNull(nameof(budgetDay), budgetDay);
			if (budgetDay.IsClosed) return new BudgetCalculationResult();
			var budgetDayCalculations = _netStaffCalculator.CalculatedResult(budgetDay);
			budgetDayCalculations.NetStaffFcAdj = netStaffFcAdj;
			foreach (var calculator in _calculatorList)
				calculator.Calculate(budgetDay, _budgetDayList, ref budgetDayCalculations);

			return budgetDayCalculations;
		}
	}
}