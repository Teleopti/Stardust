﻿using System.Globalization;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.Tracking;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.TestData.Setups.Configurable
{
	public class PersonAccountUpdateConfigurable : IUserDataSetup
	{
		public ICurrentScenario CurrentScenario { get; set; }

		public void Apply(ICurrentUnitOfWork currentUnitOfWork, IPerson user, CultureInfo cultureInfo)
		{
			var repository = new PersonAbsenceAccountRepository(currentUnitOfWork);
			var scheduleRepository = new ScheduleStorage(currentUnitOfWork, new RepositoryFactory(), new PersistableScheduleDataPermissionChecker());
			var traceableService = new TraceableRefreshService(CurrentScenario, scheduleRepository);
			var updater = new PersonAccountUpdater(repository, traceableService);
			updater.Update(user);
		}
	}
}
