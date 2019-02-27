using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.UnitOfWork;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    /// <summary>
    /// Repository for KeyPerformanceIndicator
    /// </summary>
    /// /// 
    /// <remarks>
    ///  Created by: Ola
    ///  Created date: 2008-04-07    
    /// /// </remarks>
    public class KpiRepository : Repository<IKeyPerformanceIndicator>, IKpiRepository
	{
		public static KpiRepository DONT_USE_CTOR(IUnitOfWork unitOfWork)
		{
			return new KpiRepository(new ThisUnitOfWork(unitOfWork), null, null);
		}

		public KpiRepository(ICurrentUnitOfWork currentUnitOfWork, ICurrentBusinessUnit currentBusinessUnit, Lazy<IUpdatedBy> updatedBy)
				: base(currentUnitOfWork, currentBusinessUnit, updatedBy)
	    {
		}
	}
}