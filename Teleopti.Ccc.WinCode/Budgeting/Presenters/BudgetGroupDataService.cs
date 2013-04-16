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
			var calculators = new List<ICalculator>
				{
					new ForecastedStaffCalculator(),
					new NetStaffForecastAdjustCalculator(netStaffCalculator, grossStaffCalculator),
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
				foreach (var detailModel in models) //hmm, second same loop
				{
					detailModel.Recalculate(calculator);
				}

				var daysThatHaveSurplus = models.Where(d => !d.BudgetDay.NetStaffFcAdjustedSurplus.GetValueOrDefault().Equals(0)).ToList();
				if (!daysThatHaveSurplus.Any()) return;
				foreach (var surplusDay in daysThatHaveSurplus)
				{
					var week = getWeek(surplusDay, models);
					var surplusToDistribute = week.Sum(d => d.BudgetDay.NetStaffFcAdjustedSurplus);
					var availableDays = week.Where(d => d.BudgetDay.NetStaffFcAdjustedSurplus.GetValueOrDefault().Equals(0)).ToList();
					if (!availableDays.Any()) return;
					var individualSurplus = surplusToDistribute/availableDays.Count();
					foreach (var day in availableDays)
					{
						var closedToInt = day.IsClosed ? 1 : 0;
						var maxStaff = (1 - closedToInt)*
						               (grossStaffCalculator.CalculatedResult(day.BudgetDay).GrossStaff + day.Contractors)*
						               (1 - day.BudgetDay.CustomShrinkages.GetTotal().Value);
						day.NetStaffFcAdj = NetStaffFcAdjustSurplusDistributor.Distribute(day.BudgetDay, day.NetStaffFcAdj, individualSurplus.GetValueOrDefault(), maxStaff);
					}

				}


				//var daysThatHaveFcAdjSurplus = models.Where(d => !d.BudgetDay.NetStaffFcAdjustedSurplus.GetValueOrDefault().Equals(0)).ToList();
				//if (!daysThatHaveFcAdjSurplus.Any()) return;
				//foreach (var week in daysThatHaveFcAdjSurplus.Select(day => getWeek(day, models)).Distinct().ToList())
				//{
				//	var surplusDays = week.Where(d => !d.BudgetDay.NetStaffFcAdjustedSurplus.GetValueOrDefault().Equals(0));
				//	var currentSurplusDays = surplusDays.Count();
				//	do
				//	{
				//		foreach (var day in week)
				//		{
							//var closedToInt = day.IsClosed ? 1 : 0;
							//var maxStaff = (1 - closedToInt)*
							//			   (grossStaffCalculator.CalculatedResult(day.BudgetDay).GrossStaff + day.Contractors)*
							//			   (1 - day.BudgetDay.CustomShrinkages.GetTotal().Value);

				//		}
				//	} while ();
				//}


				//foreach (var week in daysThatHaveFcAdjSurplus.Select(day => getWeek(day, models)).ToList())
				//	recalculateWeek(calculator, week, week.Count(d => d.BudgetDay.NetStaffFcAdjustedSurplus.Equals(-1D)));
			}
		}

		//private static void recalculateWeek(IBudgetCalculator calculator, IList<IBudgetGroupDayDetailModel> week, int surplusDaysCount) //, int avaiableDaysCount)
		//{
		//	week.ForEach(d => d.Recalculate(calculator));
		//	var surplusDays = week.Where(d => d.BudgetDay.NetStaffFcAdjustedSurplus.Equals(-1D)).ToList();
		//	var availableDays = week.Except(surplusDays).Where(d => !d.BudgetDay.IsClosed).ToList();
		//	if (surplusDaysCount >= surplusDays.Count || 
		//		!surplusDays.Any() || 
		//		!availableDays.Any()) 
		//		return;
		//	recalculateWeek(calculator, week, surplusDays.Count);
		//}

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