﻿using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCode.Common
{
	public class ScheduleDataLoader
	{
		private ISchedulerStateHolder _schedulerStateHolder;

		public ScheduleDataLoader(ISchedulerStateHolder schedulerStateHolder)
		{
			_schedulerStateHolder = schedulerStateHolder;
		}

		public void LoadSchedule(IUnitOfWork unitOfWork, DateTimePeriod dateTimePeriod, IPerson person)
		{
			IList<IPerson> persons = new List<IPerson> { person };
			IScheduleDateTimePeriod scheduleDateTimePeriod = new ScheduleDateTimePeriod(dateTimePeriod, persons);
			IPersonProvider personProvider = new PersonsInOrganizationProvider(persons);
		    IScheduleDictionaryLoadOptions scheduleDictionaryLoadOptions = new ScheduleDictionaryLoadOptions(true, true);
			var scheduleRepository = new ScheduleStorage(new ThisUnitOfWork(unitOfWork), new RepositoryFactory(), new PersistableScheduleDataPermissionChecker());
			_schedulerStateHolder.LoadSchedules(scheduleRepository, personProvider, scheduleDictionaryLoadOptions, scheduleDateTimePeriod);
		}
	}
}
