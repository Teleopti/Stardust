using System;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    public class BudgetGroupRepository : Repository<IBudgetGroup>, IBudgetGroupRepository
    {
        public BudgetGroupRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public BudgetGroupRepository(IUnitOfWorkFactory unitOfWorkFactory) : base(unitOfWorkFactory)
        {    
        }


    }
}
