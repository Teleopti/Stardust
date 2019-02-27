using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.UnitOfWork;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    /// <summary>
    /// Repository for masteractivities
    /// </summary>
    public class MasterActivityRepository : Repository<IMasterActivity>, IMasterActivityRepository
    {
		public static MasterActivityRepository DONT_USE_CTOR(IUnitOfWork unitOfWork)
		{
			return new MasterActivityRepository(new ThisUnitOfWork(unitOfWork), null, null);
		}

		public MasterActivityRepository(ICurrentUnitOfWork currentUnitOfWork, ICurrentBusinessUnit currentBusinessUnit, Lazy<IUpdatedBy> updatedBy)
			: base(currentUnitOfWork, currentBusinessUnit, updatedBy)
		{
		}
    }
}