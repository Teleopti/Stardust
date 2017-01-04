using Teleopti.Ccc.Domain.Repositories;
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
			var repositoryFactory = new RepositoryFactory();
			var currentUnitOfWork = new ThisUnitOfWork(unitOfWork);
			return new ScheduleStorage(currentUnitOfWork, repositoryFactory, new PersistableScheduleDataPermissionChecker(), new ScheduleStorageRepositoryWrapper(repositoryFactory, currentUnitOfWork));
		}
	}
}