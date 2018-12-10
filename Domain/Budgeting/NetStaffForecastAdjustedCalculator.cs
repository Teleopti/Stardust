using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Budgeting
{
	public class NetStaffForecastAdjustCalculator : ICalculator
	{
		private readonly INetStaffCalculator _netStaffCalculator;
		private readonly IGrossStaffCalculator _grossStaffCalculator;

		public NetStaffForecastAdjustCalculator(INetStaffCalculator netStaffCalculator, IGrossStaffCalculator grossStaffCalculator)
		{
			_netStaffCalculator = netStaffCalculator;
			_grossStaffCalculator = grossStaffCalculator;
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
			var maxStaff = getMaxStaffForDay(budgetDay);

			double netStaffFcAdj;
			if (!currentForecastedHoursWeekSum.Equals(0))
				netStaffFcAdj = (budgetDay.ForecastedHours/currentForecastedHoursWeekSum)*currentNetStaffWeekSum;
			else
				netStaffFcAdj = _netStaffCalculator.CalculatedResult(budgetDay).NetStaff;

			if (netStaffFcAdj > maxStaff)
			{
				budgetCalculationResult.NetStaffFcAdj = maxStaff;
				budgetDay.NetStaffFcAdjustedSurplus = netStaffFcAdj - maxStaff;
			}
			else
			{
				budgetCalculationResult.NetStaffFcAdj = netStaffFcAdj;
				budgetDay.NetStaffFcAdjustedSurplus = 0;
			}
			
		}

		private double getMaxStaffForDay(IBudgetDay budgetDay)
		{
			var closedToInt = budgetDay.IsClosed ? 1 : 0;
			return (1 - closedToInt) * (_grossStaffCalculator.CalculatedResult(budgetDay).GrossStaff + budgetDay.Contractors) *
						   (1 - budgetDay.CustomShrinkages.GetTotal().Value);
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
