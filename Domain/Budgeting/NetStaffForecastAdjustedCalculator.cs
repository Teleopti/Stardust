using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Budgeting
{
	public class NetStaffForecastAdjustCalculator : ICalculator
	{
		private readonly INetStaffCalculator _netStaffCalculator;

		public NetStaffForecastAdjustCalculator(INetStaffCalculator netStaffCalculator)
		{
			_netStaffCalculator = netStaffCalculator;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods",
			MessageId = "1"),
		 System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods",
			 MessageId = "2")]
		public void Calculate(IBudgetDay budgetDay, IEnumerable<IBudgetDay> budgetDayList, ref BudgetCalculationResult budgetCalculationResult)
		{
			var week = DateHelper.GetWeekPeriod(budgetDay.Day, CultureInfo.CurrentCulture);

			var budgetDaysWithinWeek = new List<IBudgetDay>(7);
			budgetDaysWithinWeek.AddRange(
				budgetDayList.TakeWhile(day => day.Day <= week.EndDate).Where(day => day.Day >= week.StartDate));

			var currentNetStaffWeekSum = getWeekSumNetStaff(budgetDaysWithinWeek);
			var currentForecastedHoursWeekSum = getWeekSumForecastedHours(budgetDaysWithinWeek);

			double netStaffFcAdj = 0;
			if (!currentForecastedHoursWeekSum.Equals(0))
				netStaffFcAdj = (budgetDay.ForecastedHours/currentForecastedHoursWeekSum)*currentNetStaffWeekSum;

			var closedToInt = budgetDay.IsClosed ? 1 : 0;
			var sumCustomShrinkage = budgetDay.CustomShrinkages.GetTotal();
			var maxStaff = (1 - closedToInt)*(budgetCalculationResult.GrossStaff + budgetDay.Contractors)*
			               (1 - (sumCustomShrinkage).Value);

			budgetCalculationResult.NetStaffFcAdj = netStaffFcAdj > maxStaff ? maxStaff : netStaffFcAdj;
		}

		private double getWeekSumNetStaff(IEnumerable<IBudgetDay> weekBudgetDays)
		{
			return
				weekBudgetDays.Select(budgetDay => _netStaffCalculator.CalculatedResult(budgetDay))
				              .Select(calculatedResult => calculatedResult.NetStaff)
				              .Sum();
		}

		private static double getWeekSumForecastedHours(IEnumerable<IBudgetDay> weekBudgetDays)
		{
			return weekBudgetDays.Sum(budgetDay => budgetDay.ForecastedHours);
		}
	}
}
