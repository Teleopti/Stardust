using System.Globalization;
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
			var repository = new PersonAbsenceAccountRepository(unitOfWork);
			var personAssignmentRepository = new PersonAssignmentRepository(unitOfWork);
			var personAbsenceRepository = new PersonAbsenceRepository(unitOfWork);
			var noteRepository = new NoteRepository(unitOfWork);
			var publicNoteRepository = new PublicNoteRepository(unitOfWork);
			var agentDayScheduleTagRepository = new AgentDayScheduleTagRepository(unitOfWork);
			var preferenceDayRepository = new PreferenceDayRepository(unitOfWork);
			var studentAvailabilityDayRepository = new StudentAvailabilityDayRepository(unitOfWork);
			var overtimeAvailabilityRepository = new OvertimeAvailabilityRepository(unitOfWork);
			var scheduleRepository = new ScheduleStorage(unitOfWork, personAssignmentRepository,
				personAbsenceRepository, new MeetingRepository(unitOfWork),
				agentDayScheduleTagRepository, noteRepository,
				publicNoteRepository, preferenceDayRepository,
				studentAvailabilityDayRepository, new PersonAvailabilityRepository(unitOfWork),
				new PersonRotationRepository(unitOfWork), overtimeAvailabilityRepository,
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
