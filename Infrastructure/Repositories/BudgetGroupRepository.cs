using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;

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
