using System;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface ISeniorityWorkDayRanksRepository : IRepository<ISeniorityWorkDayRanks>
	{
		[Obsolete("Don't use! Shouldn't be here - use ICurrentUnitOfWork instead (or get the unitofwork in some other way).")]
		IUnitOfWork UnitOfWork { get; }
	}
}