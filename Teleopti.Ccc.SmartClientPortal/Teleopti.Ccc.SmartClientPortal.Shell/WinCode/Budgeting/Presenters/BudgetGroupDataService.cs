using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting.Models;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting.Presenters
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
				}).ToArray(); //One loop

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
						new FullAllowanceCalculator(),
						new AllowanceCalculator()
					});

			IBudgetCalculator calculator = new BudgetCalculator(budgetDays, netStaffCalculator, calculators);
			using (PerformanceOutput.ForOperation("Recalculating"))
			{
				models.ForEach(d => d.Recalculate(calculator)); //hmm, second same loop
				
				NetStaffFcAdjustSurplusDistributor.Distribute(calculator, grossStaffCalculator, netStaffForecasteAdjustCalculator, models);
			}
		}
			
			[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public void UpdateBudgetDays(IBudgetDayProvider budgetDayProvider)
        {
            if (budgetDayProvider == null) throw new ArgumentNullException(nameof(budgetDayProvider));
            foreach (var budgetGroupDayDetailModel in budgetDayProvider.VisibleDayModels())
    		{
    			budgetGroupDayDetailModel.UpdateBudgetDay();
    		}
        }

        private void SaveUpdatedBudgetDays(IBudgetDayProvider budgetDayProvider)
    	{
            if (budgetDayProvider == null) throw new ArgumentNullException(nameof(budgetDayProvider));
            foreach (var budgetGroupDayDetailModel in budgetDayProvider.VisibleDayModels().Where(b => b.BudgetDay.Id.HasValue))
            {
                budgetGroupDayDetailModel.BudgetDay =
#pragma warning disable 618
                    _budgetDayRepository.UnitOfWork.Merge(budgetGroupDayDetailModel.BudgetDay);
#pragma warning restore 618
            }
    	}

        private void SaveNewBudgetDays(IBudgetDayProvider budgetDayProvider)
        {
            var newBudgetDays = budgetDayProvider.VisibleDayModels().Where(b => !b.BudgetDay.Id.HasValue).ToArray();
            foreach (var budgetGroupDayDetailModel in newBudgetDays)
            {
                _budgetDayRepository.Add(budgetGroupDayDetailModel.BudgetDay);
            }
        }
    }
}