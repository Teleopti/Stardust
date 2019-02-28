﻿using System.Globalization;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Security.Principal;
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
			var repository = PersonAbsenceAccountRepository.DONT_USE_CTOR(unitOfWork);
			var personAssignmentRepository = PersonAssignmentRepository.DONT_USE_CTOR(unitOfWork);
			var personAbsenceRepository = new PersonAbsenceRepository(unitOfWork);
			var noteRepository = new NoteRepository(unitOfWork);
			var publicNoteRepository = PublicNoteRepository.DONT_USE_CTOR(unitOfWork);
			var agentDayScheduleTagRepository = AgentDayScheduleTagRepository.DONT_USE_CTOR(unitOfWork);
			var preferenceDayRepository = new PreferenceDayRepository(unitOfWork);
			var studentAvailabilityDayRepository = StudentAvailabilityDayRepository.DONT_USE_CTOR(unitOfWork);
			var overtimeAvailabilityRepository = new OvertimeAvailabilityRepository(unitOfWork);
			var scheduleRepository = new ScheduleStorage(unitOfWork, personAssignmentRepository,
				personAbsenceRepository, new MeetingRepository(unitOfWork),
				agentDayScheduleTagRepository, noteRepository,
				publicNoteRepository, preferenceDayRepository,
				studentAvailabilityDayRepository, PersonAvailabilityRepository.DONT_USE_CTOR(unitOfWork),
				PersonRotationRepository.DONT_USE_CTOR(unitOfWork), overtimeAvailabilityRepository,
				new PersistableScheduleDataPermissionChecker(CurrentAuthorization.Make()),
				new ScheduleStorageRepositoryWrapper(() => personAssignmentRepository,
					() => personAbsenceRepository,
					() => preferenceDayRepository, () => noteRepository,
					() => publicNoteRepository,
					() => studentAvailabilityDayRepository,
					() => agentDayScheduleTagRepository,
					() => overtimeAvailabilityRepository), CurrentAuthorization.Make());
			var traceableService = new TraceableRefreshService(CurrentScenario, scheduleRepository);
			var updater = new PersonAccountUpdater(repository, traceableService);
			updater.Update(person);
		}
	}
}
