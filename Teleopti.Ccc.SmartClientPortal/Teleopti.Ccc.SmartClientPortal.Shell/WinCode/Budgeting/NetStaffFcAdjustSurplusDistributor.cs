using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting.Models;
using Teleopti.Interfaces.Domain;

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
			                     let week = getWeekWithoutClosedDays(surplusDay, models)
			                     where daysThatHaveSurplus.Contains(surplusDay)
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
			var budgetDaysWithinWeek = new List<IBudgetGroupDayDetailModel>(7);
			budgetDaysWithinWeek.AddRange(
				budgetDayList.TakeWhile(day => day.BudgetDay.Day <= week.EndDate)
				             .Where(day => day.BudgetDay.Day >= week.StartDate && !day.BudgetDay.IsClosed));
			return budgetDaysWithinWeek;
		}

		private static void distributeSurplus(IList<IBudgetGroupDayDetailModel> week,
		                                      IGrossStaffCalculator grossStaffCalculator,
		                                      int availableDaysPrevious,
		                                      double surplus)
		{
			var avaiableDays = week.Where(d => d.BudgetDay.NetStaffFcAdjustedSurplus.GetValueOrDefault().Equals(0)).ToList();
			var surplusDays = week.Except(avaiableDays).ToList();
			var surplusToDistribute = surplusDays.Sum(d => d.BudgetDay.NetStaffFcAdjustedSurplus.GetValueOrDefault()) + surplus;

			foreach (var day in avaiableDays)
			{
				var closedToInt = day.BudgetDay.IsClosed ? 1 : 0;
				var maxStaff = (1 - closedToInt)*
				               (grossStaffCalculator.CalculatedResult(day.BudgetDay).GrossStaff + day.Contractors)*
				               (1 - day.BudgetDay.CustomShrinkages.GetTotal().Value);
				day.NetStaffFcAdj = recalculateNetStaffFcAdj(day.BudgetDay, day.NetStaffFcAdj,
				                                             surplusToDistribute/avaiableDays.Count(),
				                                             maxStaff);
			}
			
			avaiableDays = week.Where(d => d.BudgetDay.NetStaffFcAdjustedSurplus.GetValueOrDefault().Equals(0)).ToList();

			if (avaiableDays.Count == availableDaysPrevious)
				return;
			distributeSurplus(avaiableDays, grossStaffCalculator, avaiableDays.Count,
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
