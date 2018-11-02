using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.UnitOfWork;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class ScheduleStorageFactory : IScheduleStorageFactory
	{
		public IScheduleStorage Create(IUnitOfWork unitOfWork)
		{
			var repositoryFactory = new RepositoryFactory();
			var currentUnitOfWork = new ThisUnitOfWork(unitOfWork);
			var authorization = CurrentAuthorization.Make();
			return new ScheduleStorage(currentUnitOfWork, repositoryFactory,
				new PersistableScheduleDataPermissionChecker(authorization),
				new ScheduleStorageRepositoryWrapper(repositoryFactory, currentUnitOfWork), authorization);
		}
	}
}