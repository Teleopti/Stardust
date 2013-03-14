using System;
using System.Linq;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Generic
{
    public class BudgetdayConfigurable : IDataSetup
    {
        private readonly string _budgetGroupName;
        private readonly DateTime _date;

        public BudgetdayConfigurable(string budgetGroupName, DateTime date)
        {
            _budgetGroupName = budgetGroupName;
            _date = date;
        }

        public void Apply(IUnitOfWork uow)
        {
            var scenarioRepository = new ScenarioRepository(uow);
            var budgetDayRepository = new BudgetDayRepository(uow);
            var budgetGroupRepository = new BudgetGroupRepository(uow);
            var budgetGroup = budgetGroupRepository.LoadAll().First(b => b.Name == _budgetGroupName);
            var budgetDay = new BudgetDay(budgetGroup, scenarioRepository.LoadDefaultScenario(), new DateOnly(_date));
            budgetDayRepository.Add(budgetDay);
        }
    }
}