using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.WinCode.Budgeting.Models;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Budgeting.Presenters
{
    public class BudgetGroupDataService : IBudgetGroupDataService
    {
        private readonly BudgetGroupMainModel _budgetGroupMainModel;
        private readonly IBudgetDayRepository _budgetDayRepository;
        private readonly IBudgetDayGapFiller _budgetDayGapFiller;
        private readonly IBudgetPermissionService _budgetPermissionService;

        public BudgetGroupDataService(BudgetGroupMainModel budgetGroupMainModel, IBudgetDayRepository budgetDayRepository, IBudgetDayGapFiller budgetDayGapFiller, IBudgetPermissionService budgetPermissionService)
        {
            _budgetGroupMainModel = budgetGroupMainModel;
            _budgetDayRepository = budgetDayRepository;
            _budgetDayGapFiller = budgetDayGapFiller;
            _budgetPermissionService = budgetPermissionService;
        }

        public IList<IBudgetGroupDayDetailModel> FindAndCreate()
        {
            var expandedPeriodStartDate = _budgetDayRepository.FindLastDayWithStaffEmployed(_budgetGroupMainModel.Scenario, _budgetGroupMainModel.BudgetGroup,
                                                                                            _budgetGroupMainModel.Period.StartDate);
            var expandedPeriod = new DateOnlyPeriod(expandedPeriodStartDate, _budgetGroupMainModel.Period.EndDate);
            var existingBudgetDays = _budgetDayRepository.Find(_budgetGroupMainModel.Scenario, _budgetGroupMainModel.BudgetGroup, expandedPeriod);
            var allBudgetDaysForPeriod = _budgetDayGapFiller.AddMissingDays(existingBudgetDays, expandedPeriod);

            var models = new List<IBudgetGroupDayDetailModel>();
            allBudgetDaysForPeriod.ForEach(b => models.Add(new BudgetGroupDayDetailModel(b)));

            return models;
        }

        public void Save(IBudgetDayProvider budgetDayProvider)
        {
            UpdateBudgetDays(budgetDayProvider);
            SaveUpdatedBudgetDays(budgetDayProvider);
            SaveNewBudgetDays(budgetDayProvider);
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void Recalculate(IList<IBudgetGroupDayDetailModel> models)
		{
			var budgetDays = models.Select(dayDetailModel =>
				{
					dayDetailModel.UpdateBudgetDay();
					return dayDetailModel.BudgetDay;
				}).ToList(); //One loop

			var grossStaffCalculator = new GrossStaffCalculator();
			var netStaffCalculator = new NetStaffCalculator(grossStaffCalculator);
			var netStaffForecasteAdjustCalculator = new NetStaffForecastAdjustCalculator(netStaffCalculator, grossStaffCalculator);
			var calculators = new List<ICalculator>
				{
					new ForecastedStaffCalculator(),
					netStaffForecasteAdjustCalculator,
					new BudgetedStaffCalculator(),
					new DifferenceCalculator(),
					new DifferencePercentCalculator()
				};

			if (_budgetPermissionService.IsAllowancePermitted)
				calculators.AddRange(new List<ICalculator>
					{
						new BudgetedLeaveCalculator(netStaffCalculator),
						new BudgetedSurplusCalculator(),
						new TotalAllowanceCalculator(),
						new AllowanceCalculator()
					});

			IBudgetCalculator calculator = new BudgetCalculator(budgetDays, netStaffCalculator, calculators);
			using (PerformanceOutput.ForOperation("Recalculating"))
			{
				models.ForEach(d => d.Recalculate(calculator)); //hmm, second same loop
				
				var daysThatHaveSurplus = models.Where(d => !d.BudgetDay.NetStaffFcAdjustedSurplus.GetValueOrDefault().Equals(0)).ToList();
				if (!daysThatHaveSurplus.Any()) return;
				foreach (var surplusDay in daysThatHaveSurplus)
				{
					var week = getWeek(surplusDay, models);
					var surplusDays = week.Where(d => !d.BudgetDay.NetStaffFcAdjustedSurplus.GetValueOrDefault().Equals(0)).ToList();
					distributeSurplus(week, grossStaffCalculator, surplusDays.Count(), week.Except(surplusDays).Count());
				}
				calculator.CalculatorList.Remove(netStaffForecasteAdjustCalculator);
				models.ForEach(d => d.RecalculateWithoutNetStaffForecastAdjustCalculator(calculator, d.NetStaffFcAdj));
			}
		}

		private static void distributeSurplus(IList<IBudgetGroupDayDetailModel> week, IGrossStaffCalculator grossStaffCalculator, int surplusDaysPrevious, int availableDaysPrevious)
		{
			var surplusDays = week.Where(d => !d.BudgetDay.NetStaffFcAdjustedSurplus.GetValueOrDefault().Equals(0)).ToList();
			var avaiableDays = week.Except(surplusDays).ToList();
			var surplusToDistribute = surplusDays.Sum(d => d.BudgetDay.NetStaffFcAdjustedSurplus.GetValueOrDefault());
			foreach (var day in avaiableDays)
			{
				var closedToInt = day.IsClosed ? 1 : 0;
				var maxStaff = (1 - closedToInt)*
				               (grossStaffCalculator.CalculatedResult(day.BudgetDay).GrossStaff + day.Contractors)*
				               (1 - day.BudgetDay.CustomShrinkages.GetTotal().Value);
				day.NetStaffFcAdj = NetStaffFcAdjustSurplusDistributor.Distribute(day.BudgetDay, day.NetStaffFcAdj, surplusToDistribute/avaiableDays.Count(), maxStaff);
			}
			if (surplusDays.Count() == surplusDaysPrevious && avaiableDays.Count() == availableDaysPrevious)
				return;
			distributeSurplus(week, grossStaffCalculator, surplusDays.Count, avaiableDays.Count);
		}
		
		private static IList<IBudgetGroupDayDetailModel> getWeek(IBudgetGroupDayDetailModel budgetDay, IEnumerable<IBudgetGroupDayDetailModel> budgetDayList)
		{
			var week = DateHelper.GetWeekPeriod(budgetDay.BudgetDay.Day, CultureInfo.CurrentCulture);
			var budgetDaysWithinWeek = new List<IBudgetGroupDayDetailModel>(7);
			budgetDaysWithinWeek.AddRange(
				budgetDayList.TakeWhile(day => day.BudgetDay.Day <= week.EndDate).Where(day => day.BudgetDay.Day >= week.StartDate));
			return budgetDaysWithinWeek;
		}
			
			[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public void UpdateBudgetDays(IBudgetDayProvider budgetDayProvider)
        {
            if (budgetDayProvider == null) throw new ArgumentNullException("budgetDayProvider");
            foreach (var budgetGroupDayDetailModel in budgetDayProvider.VisibleDayModels())
    		{
    			budgetGroupDayDetailModel.UpdateBudgetDay();
    		}
        }

        private void SaveUpdatedBudgetDays(IBudgetDayProvider budgetDayProvider)
    	{
            if (budgetDayProvider == null) throw new ArgumentNullException("budgetDayProvider");
            foreach (var budgetGroupDayDetailModel in budgetDayProvider.VisibleDayModels().Where(b => b.BudgetDay.Id.HasValue))
            {
                budgetGroupDayDetailModel.BudgetDay =
                    _budgetDayRepository.UnitOfWork.Merge(budgetGroupDayDetailModel.BudgetDay);
            }
    	}

        private void SaveNewBudgetDays(IBudgetDayProvider budgetDayProvider)
        {
            var newBudgetDays = budgetDayProvider.VisibleDayModels().Where(b => !b.BudgetDay.Id.HasValue).ToList();
            foreach (var budgetGroupDayDetailModel in newBudgetDays)
            {
                _budgetDayRepository.Add(budgetGroupDayDetailModel.BudgetDay);
            }
        }
    }
}