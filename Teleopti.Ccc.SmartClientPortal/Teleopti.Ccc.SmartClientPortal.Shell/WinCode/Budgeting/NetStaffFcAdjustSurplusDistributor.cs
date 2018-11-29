using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting.Models;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting
{
	public static class NetStaffFcAdjustSurplusDistributor
	{
		public static void Distribute(IBudgetCalculator calculator,
		                              IGrossStaffCalculator grossStaffCalculator,
		                              ICalculator netStaffForecastAdjustCalculator,
		                              IList<IBudgetGroupDayDetailModel> models)
		{
			var daysThatHaveSurplus =
				models.Where(d => !d.BudgetDay.NetStaffFcAdjustedSurplus.GetValueOrDefault().Equals(0) && !d.BudgetDay.IsClosed)
				      .ToList();
			if (!daysThatHaveSurplus.Any()) return;
			
			// ReSharper disable AccessToModifiedClosure

			// ErikS: For any non-LINQ lovers: Loop through distinct 
			// weeks that have one or more days with a FcAdj surplus
			foreach (var week in from surplusDay in daysThatHaveSurplus
			                     where daysThatHaveSurplus.Contains(surplusDay)
								 let week = getWeekWithoutClosedDays(surplusDay, models)
			                     select week)
			{
				distributeSurplus(week, grossStaffCalculator,
				                  week.Count(d => d.BudgetDay.NetStaffFcAdjustedSurplus.GetValueOrDefault().Equals(0)),
				                  0);
				// remove the week we just counted so that we dont count a week more than once
				daysThatHaveSurplus = daysThatHaveSurplus.Except(week).ToList();
			}
			calculator.CalculatorList.Remove(netStaffForecastAdjustCalculator);
			models.ForEach(d => d.RecalculateWithoutNetStaffForecastAdjustCalculator(calculator, d.NetStaffFcAdj));

			// ReSharper restore AccessToModifiedClosure
		}

		private static IList<IBudgetGroupDayDetailModel> getWeekWithoutClosedDays(IBudgetGroupDayDetailModel budgetDay,
		                                                                          IEnumerable<IBudgetGroupDayDetailModel>
			                                                                          budgetDayList)
		{
			var week = DateHelper.GetWeekPeriod(budgetDay.BudgetDay.Day, CultureInfo.CurrentCulture);
			var budgetDaysWithinWeek = budgetDayList.TakeWhile(day => day.BudgetDay.Day <= week.EndDate)
				             .Where(day => day.BudgetDay.Day >= week.StartDate && !day.BudgetDay.IsClosed).ToList();
			return budgetDaysWithinWeek;
		}

		private static void distributeSurplus(IList<IBudgetGroupDayDetailModel> week,
		                                      IGrossStaffCalculator grossStaffCalculator,
		                                      int availableDaysPrevious,
		                                      double surplus)
		{
			var availableDays = week.Where(d => d.BudgetDay.NetStaffFcAdjustedSurplus.GetValueOrDefault().Equals(0)).ToList();
			var surplusDays = week.Except(availableDays).ToList();
			var surplusToDistribute = surplusDays.Sum(d => d.BudgetDay.NetStaffFcAdjustedSurplus.GetValueOrDefault()) + surplus;

			foreach (var day in availableDays)
			{
				var closedToInt = day.BudgetDay.IsClosed ? 1 : 0;
				var maxStaff = (1 - closedToInt)*
				               (grossStaffCalculator.CalculatedResult(day.BudgetDay).GrossStaff + day.Contractors)*
				               (1 - day.BudgetDay.CustomShrinkages.GetTotal().Value);
				day.NetStaffFcAdj = recalculateNetStaffFcAdj(day.BudgetDay, day.NetStaffFcAdj,
				                                             surplusToDistribute/availableDays.Count,
				                                             maxStaff);
			}
			
			availableDays = week.Where(d => d.BudgetDay.NetStaffFcAdjustedSurplus.GetValueOrDefault().Equals(0)).ToList();

			if (availableDays.Count == availableDaysPrevious)
				return;
			distributeSurplus(availableDays, grossStaffCalculator, availableDays.Count,
			                  week.Except(surplusDays).Sum(d => d.BudgetDay.NetStaffFcAdjustedSurplus.GetValueOrDefault()));
		}

		private static double recalculateNetStaffFcAdj(IBudgetDay budgetDay, double currentNetStaffFcAdj,
		                                               double surplusToDistribute,
		                                               double maxStaff)
		{
			var sumNetStaffFcAdj = currentNetStaffFcAdj + surplusToDistribute;
			if (sumNetStaffFcAdj > maxStaff)
			{
				budgetDay.NetStaffFcAdjustedSurplus = sumNetStaffFcAdj - maxStaff;
				return maxStaff;
			}
			budgetDay.NetStaffFcAdjustedSurplus = 0;
			return sumNetStaffFcAdj;
		}
	}
}
