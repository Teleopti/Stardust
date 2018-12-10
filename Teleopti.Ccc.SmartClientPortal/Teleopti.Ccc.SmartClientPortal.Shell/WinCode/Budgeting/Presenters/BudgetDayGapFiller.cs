using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting.Models;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting.Presenters
{
    public class BudgetDayGapFiller : IBudgetDayGapFiller
    {
        private readonly BudgetGroupMainModel _budgetGroupMainModel;
        
        public BudgetDayGapFiller(BudgetGroupMainModel budgetGroupMainModel)
        {
            _budgetGroupMainModel = budgetGroupMainModel;
        }

        public IList<IBudgetDay> AddMissingDays(IEnumerable<IBudgetDay> existingBudgetDays, DateOnlyPeriod period)
        {
	        var lookup = existingBudgetDays.ToDictionary(b => b.Day);
            var dayCollection = period.DayCollection();
            var allBudgetDaysForPeriod = new List<IBudgetDay>(dayCollection.Count);

            foreach (DateOnly date in dayCollection)
            {
                IBudgetDay budgetDay;
                if (!lookup.TryGetValue(date, out budgetDay))
                {
                    budgetDay = new BudgetDay(_budgetGroupMainModel.BudgetGroup, _budgetGroupMainModel.Scenario, date);
                    initiateBudgetDayWithDefaultValue(budgetDay);
                }
                allBudgetDaysForPeriod.Add(budgetDay);
            }

            return allBudgetDaysForPeriod;
        }

        private static void initiateBudgetDayWithDefaultValue(IBudgetDay budgetDay)
        {
            budgetDay.AbsenceThreshold = new Percent(1.0);
        }
    }
}