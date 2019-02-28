using System;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Ccc.TestCommon.TestData.Setups.Default;


namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Configurable
{
	public class BudgetdayConfigurable : IUserDataSetup
	{
		public string BudgetGroup { get; set; }
		public DateTime Date { get; set; }
		public int Allowance { get; set; }
		public int FulltimeEquivalentHours { get; set; }

		public void Apply(ICurrentUnitOfWork unitOfWork, IPerson person, CultureInfo cultureInfo)
		{
			var budgetDayRepository = new BudgetDayRepository(unitOfWork);
			var budgetGroupRepository = BudgetGroupRepository.DONT_USE_CTOR(unitOfWork);
			IBudgetGroup budgetGroup = new BudgetGroup() { Name = BudgetGroup };
			var existingBudgetGroups = budgetGroupRepository.LoadAll();
			bool budgetGroupAlreadyExists = existingBudgetGroups.Any(b => b.Name == BudgetGroup);
			if (budgetGroupAlreadyExists)
			{
				budgetGroup = existingBudgetGroups.First(b => b.Name == BudgetGroup);
			}
			else
			{
				budgetGroup = new BudgetGroup() { Name = BudgetGroup, TimeZone = person.PermissionInformation.DefaultTimeZone() };
				budgetGroupRepository.Add(budgetGroup);
			}
			person.PersonPeriodCollection.First().BudgetGroup = budgetGroup;
			var scenario = DefaultScenario.Scenario;
			var budgetDay = new BudgetDay(budgetGroup, scenario, new DateOnly(Date)) { ShrinkedAllowance = Allowance, FulltimeEquivalentHours = FulltimeEquivalentHours };
			budgetDayRepository.Add(budgetDay);
		}
	}
}