using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.UnitOfWork;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    public class KpiTargetRepository : Repository<IKpiTarget>
    {
		public static KpiTargetRepository DONT_USE_CTOR(IUnitOfWork unitOfWork)
		{
			return new KpiTargetRepository(new ThisUnitOfWork(unitOfWork), null, null);
		}

		public KpiTargetRepository(ICurrentUnitOfWork currentUnitOfWork, ICurrentBusinessUnit currentBusinessUnit, Lazy<IUpdatedBy> updatedBy) 
            : base(currentUnitOfWork, currentBusinessUnit, updatedBy)
        {
        }
    }
}
