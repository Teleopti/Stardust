using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.UnitOfWork;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class BudgetGroupRepository : Repository<IBudgetGroup>, IBudgetGroupRepository
	{
		public static BudgetGroupRepository DONT_USE_CTOR(ICurrentUnitOfWork currentUnitOfWork)
		{
			return new BudgetGroupRepository(currentUnitOfWork, null, null);
		}

		public static BudgetGroupRepository DONT_USE_CTOR(IUnitOfWork unitOfWork)
		{
			return new BudgetGroupRepository(new ThisUnitOfWork(unitOfWork), null, null);
		}

		public BudgetGroupRepository(ICurrentUnitOfWork currentUnitOfWork, ICurrentBusinessUnit currentBusinessUnit, Lazy<IUpdatedBy> updatedBy)
			: base(currentUnitOfWork, currentBusinessUnit, updatedBy)
		{
		}
	}
}