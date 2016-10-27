using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class ScheduleStorageFactory : IScheduleStorageFactory
	{
		public IScheduleStorage Create(IUnitOfWork unitOfWork)
		{
			return new ScheduleStorage(new ThisUnitOfWork(unitOfWork), new RepositoryFactory(), new PersistableScheduleDataPermissionChecker());
		}
	}
}