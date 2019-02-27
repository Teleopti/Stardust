using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Repositories;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common
{
	public class ScheduleDataLoader
	{
		public IScheduleDictionary LoadSchedule(IUnitOfWork unitOfWork, DateTimePeriod dateTimePeriod, IPerson person, IScenario scenario)
		{
			IList<IPerson> persons = new List<IPerson> { person };
		    var scheduleDictionaryLoadOptions = new ScheduleDictionaryLoadOptions(true, true);
			var currentUnitOfWork = new ThisUnitOfWork(unitOfWork);
			var personAssignmentRepository = PersonAssignmentRepository.DONT_USE_CTOR(currentUnitOfWork);
			var personAbsenceRepository = new PersonAbsenceRepository(currentUnitOfWork);
			var noteRepository = new NoteRepository(currentUnitOfWork);
			var preferenceDayRepository = new PreferenceDayRepository(currentUnitOfWork);
			var publicNoteRepository = new PublicNoteRepository(currentUnitOfWork);
			var studentAvailabilityDayRepository = new StudentAvailabilityDayRepository(currentUnitOfWork);
			var agentDayScheduleTagRepository = AgentDayScheduleTagRepository.DONT_USE_CTOR(currentUnitOfWork);
			var overtimeAvailabilityRepository = new OvertimeAvailabilityRepository(currentUnitOfWork);
			var scheduleRepository = new ScheduleStorage(currentUnitOfWork,
				personAssignmentRepository, personAbsenceRepository,
				new MeetingRepository(currentUnitOfWork), agentDayScheduleTagRepository,
				noteRepository, publicNoteRepository,
				preferenceDayRepository, studentAvailabilityDayRepository,
				PersonAvailabilityRepository.DONT_USE_CTOR(currentUnitOfWork), new PersonRotationRepository(currentUnitOfWork),
				overtimeAvailabilityRepository,
				new PersistableScheduleDataPermissionChecker(CurrentAuthorization.Make()),
				new ScheduleStorageRepositoryWrapper(() => personAssignmentRepository,
					() => personAbsenceRepository,
					() => preferenceDayRepository, () => noteRepository,
					() => publicNoteRepository,
					() => studentAvailabilityDayRepository,
					() => agentDayScheduleTagRepository,
					() => overtimeAvailabilityRepository),
				CurrentAuthorization.Make());
			return scheduleRepository.FindSchedulesForPersons(scenario, persons, scheduleDictionaryLoadOptions, dateTimePeriod, persons, true);
		}
	}
}
