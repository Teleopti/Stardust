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
			var currentUnitOfWork = new ThisUnitOfWork(unitOfWork);
			var authorization = CurrentAuthorization.Make();
			var personAssignmentRepository = new PersonAssignmentRepository(currentUnitOfWork);
			var personAbsenceRepository = new PersonAbsenceRepository(currentUnitOfWork);
			var noteRepository = new NoteRepository(currentUnitOfWork);
			var publicNoteRepository = new PublicNoteRepository(currentUnitOfWork);
			var preferenceDayRepository = new PreferenceDayRepository(currentUnitOfWork);
			var studentAvailabilityDayRepository = new StudentAvailabilityDayRepository(currentUnitOfWork);
			var agentDayScheduleTagRepository = new AgentDayScheduleTagRepository(currentUnitOfWork);
			var overtimeAvailabilityRepository = new OvertimeAvailabilityRepository(currentUnitOfWork);
			return new ScheduleStorage(currentUnitOfWork, personAssignmentRepository,
				personAbsenceRepository, new MeetingRepository(currentUnitOfWork),
				agentDayScheduleTagRepository, noteRepository,
				publicNoteRepository, preferenceDayRepository,
				studentAvailabilityDayRepository,
				new PersonAvailabilityRepository(currentUnitOfWork), new PersonRotationRepository(currentUnitOfWork),
				overtimeAvailabilityRepository,
				new PersistableScheduleDataPermissionChecker(authorization),
				new ScheduleStorageRepositoryWrapper(() => personAssignmentRepository,
					() => personAbsenceRepository,
					() => preferenceDayRepository, () => noteRepository,
					() => publicNoteRepository,
					() => studentAvailabilityDayRepository,
					() => agentDayScheduleTagRepository,
					() => overtimeAvailabilityRepository), authorization);
		}
	}
}