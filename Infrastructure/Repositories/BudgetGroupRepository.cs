using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    public class BudgetGroupRepository : Repository<IBudgetGroup>, IBudgetGroupRepository
    {
        public BudgetGroupRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

#pragma warning disable 618
        public BudgetGroupRepository(IUnitOfWorkFactory unitOfWorkFactory) : base(unitOfWorkFactory)
#pragma warning restore 618
        {    
        }

				public BudgetGroupRepository(ICurrentUnitOfWork currentUnitOfWork)
					: base(currentUnitOfWork)
	    {
		    
	    }
    }
}
