﻿using System.Globalization;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Tracking;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;

namespace Teleopti.Ccc.TestCommon.TestData.Setups.Configurable
{
	public class PersonAccountUpdateConfigurable : IUserDataSetup
	{
		public ICurrentScenario CurrentScenario { get; set; }

		public void Apply(ICurrentUnitOfWork unitOfWork, IPerson person, CultureInfo cultureInfo)
		{
			var repository = new PersonAbsenceAccountRepository(unitOfWork);
			var repositoryFactory = new RepositoryFactory();
			var scheduleRepository = new ScheduleStorage(unitOfWork, repositoryFactory, new PersistableScheduleDataPermissionChecker(), new ScheduleStorageRepositoryWrapper(repositoryFactory, unitOfWork));
			var traceableService = new TraceableRefreshService(CurrentScenario, scheduleRepository);
			var updater = new PersonAccountUpdater(repository, traceableService);
			updater.Update(person);
		}
	}
}
