using System;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Common;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Generic
{
    public class BudgetdayConfigurable : IUserDataSetup
    {

		public string BudgetGroup { get; set; }
		public DateTime Date { get; set; }
		public int Allowance { get; set; }
		public int FulltimeEquivalentHours { get; set; }


	    public void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo)
	    {
		    var budgetDayRepository = new BudgetDayRepository(uow);
            var budgetGroupRepository = new BudgetGroupRepository(uow);
			IBudgetGroup budgetGroup = new BudgetGroup() {Name = BudgetGroup };
	        var existingBudgetGroups = budgetGroupRepository.LoadAll();
			bool budgetGroupAlreadyExists = existingBudgetGroups.Any(b => b.Name == BudgetGroup);
			if (budgetGroupAlreadyExists)
			{
				budgetGroup = existingBudgetGroups.First(b => b.Name == BudgetGroup);
			}
			else
			{
				budgetGroup = new BudgetGroup() { Name = BudgetGroup, TimeZone = user.PermissionInformation.DefaultTimeZone() };
				budgetGroupRepository.Add(budgetGroup);
			}
		    user.PersonPeriodCollection.First().BudgetGroup = budgetGroup;
			var scenario = GlobalDataContext.Data().Data<CommonScenario>().Scenario;
			var budgetDay = new BudgetDay(budgetGroup, scenario, new DateOnly(Date)) { Allowance = Allowance, FulltimeEquivalentHours = FulltimeEquivalentHours };
            budgetDayRepository.Add(budgetDay);
	    }
    }
}