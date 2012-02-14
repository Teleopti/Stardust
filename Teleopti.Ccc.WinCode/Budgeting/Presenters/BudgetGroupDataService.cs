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
            var bList = models.Select(dayDetailModel =>
                                      	{
											dayDetailModel.UpdateBudgetDay();
                                      		return dayDetailModel.BudgetDay;
                                      	}).ToList(); //One loop

		    var netStaffCalculator = new NetStaffCalculator(new GrossStaffCalculator());
		    var cList = new List<ICalculator>{new DifferencePercentCalculator(netStaffCalculator, CultureInfo.CurrentCulture)};
		    if (_budgetPermissionService.IsAllowancePermitted)
                cList.Add(new AllowanceCalculator(netStaffCalculator, CultureInfo.CurrentCulture));

            IBudgetCalculator calculator = new BudgetCalculator(bList, netStaffCalculator, cList);
			using (PerformanceOutput.ForOperation("Recalculating"))
            {
                foreach (var detailModel in models) //hmm, second same loop
                {
                    detailModel.Recalculate(calculator);
                }
            }
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