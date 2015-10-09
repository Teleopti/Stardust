using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    public class BudgetGroupRepository : Repository<IBudgetGroup>, IBudgetGroupRepository
    {
#pragma warning disable 618
        public BudgetGroupRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }
#pragma warning restore 618

				public BudgetGroupRepository(ICurrentUnitOfWork currentUnitOfWork)
					: base(currentUnitOfWork)
	    {
		    
	    }
    }
}
