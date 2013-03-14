using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Generic
{
    public class BudgetGroupConfigurable : IDataSetup
    {
        private readonly string _name;

        public BudgetGroupConfigurable(string name)
        {
            _name = name;
        }

        public void Apply(IUnitOfWork uow)
        {
            var repository = new BudgetGroupRepository(uow);
            var budgetGroup = new BudgetGroup() {Name = _name};
            repository.Add(budgetGroup);
        }
    }
}