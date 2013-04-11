using System.Globalization;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Generic
{
    public class BudgetGroupConfigurable : IUserDataSetup
    {
        private readonly string _name;

        public BudgetGroupConfigurable(string name)
        {
            _name = name;
        }

	    public void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo)
	    {
			var repository = new BudgetGroupRepository(uow);
			var budgetGroup = new BudgetGroup() { Name = _name, TimeZone = user.PermissionInformation.DefaultTimeZone()};
			repository.Add(budgetGroup);
	    }
    }
}