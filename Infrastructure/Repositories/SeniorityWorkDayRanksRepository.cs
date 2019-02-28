using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.UnitOfWork;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class SeniorityWorkDayRanksRepository : Repository<ISeniorityWorkDayRanks>, ISeniorityWorkDayRanksRepository
	{
		public static SeniorityWorkDayRanksRepository DONT_USE_CTOR(IUnitOfWork unitOfWork)
		{
			return new SeniorityWorkDayRanksRepository(new ThisUnitOfWork(unitOfWork), null, null);
		}
		
		private SeniorityWorkDayRanksRepository(ICurrentUnitOfWork currentUnitOfWork, ICurrentBusinessUnit currentBusinessUnit, Lazy<IUpdatedBy> updatedBy)
			: base(currentUnitOfWork, currentBusinessUnit, updatedBy)
		{
		}
	}
}
