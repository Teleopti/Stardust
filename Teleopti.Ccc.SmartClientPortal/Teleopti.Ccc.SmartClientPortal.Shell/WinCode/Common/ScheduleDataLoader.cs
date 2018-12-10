using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Repositories;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common
{
	public class ScheduleDataLoader
	{
		private readonly ISchedulerStateHolder _schedulerStateHolder;

		public ScheduleDataLoader(ISchedulerStateHolder schedulerStateHolder)
		{
			_schedulerStateHolder = schedulerStateHolder;
		}

		public void LoadSchedule(IUnitOfWork unitOfWork, DateTimePeriod dateTimePeriod, IPerson person)
		{
			IList<IPerson> persons = new List<IPerson> { person };
		    var scheduleDictionaryLoadOptions = new ScheduleDictionaryLoadOptions(true, true);
			var repositoryFactory = new RepositoryFactory();
			var currentUnitOfWork = new ThisUnitOfWork(unitOfWork);
			var scheduleRepository = new ScheduleStorage(currentUnitOfWork, repositoryFactory, new PersistableScheduleDataPermissionChecker(CurrentAuthorization.Make()), new ScheduleStorageRepositoryWrapper(repositoryFactory, currentUnitOfWork), CurrentAuthorization.Make());
			_schedulerStateHolder.LoadSchedules(scheduleRepository, persons, scheduleDictionaryLoadOptions, dateTimePeriod);
		}
	}
}
