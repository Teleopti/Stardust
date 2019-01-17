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
		private readonly IPersonAssignmentRepository _personAssignmentRepository;

		public ScheduleStorageFactory(IPersonAssignmentRepository personAssignmentRepository)
		{
			_personAssignmentRepository = personAssignmentRepository;
		}
		
		public IScheduleStorage Create(IUnitOfWork unitOfWork)
		{
			var currentUnitOfWork = new ThisUnitOfWork(unitOfWork);
			var authorization = CurrentAuthorization.Make();
			var personAbsenceRepository = new PersonAbsenceRepository(currentUnitOfWork);
			var noteRepository = new NoteRepository(currentUnitOfWork);
			var publicNoteRepository = new PublicNoteRepository(currentUnitOfWork);
			var preferenceDayRepository = new PreferenceDayRepository(currentUnitOfWork);
			var studentAvailabilityDayRepository = new StudentAvailabilityDayRepository(currentUnitOfWork);
			var agentDayScheduleTagRepository = new AgentDayScheduleTagRepository(currentUnitOfWork);
			var overtimeAvailabilityRepository = new OvertimeAvailabilityRepository(currentUnitOfWork);
			return new ScheduleStorage(currentUnitOfWork, _personAssignmentRepository,
				personAbsenceRepository, new MeetingRepository(currentUnitOfWork),
				agentDayScheduleTagRepository, noteRepository,
				publicNoteRepository, preferenceDayRepository,
				studentAvailabilityDayRepository,
				new PersonAvailabilityRepository(currentUnitOfWork), new PersonRotationRepository(currentUnitOfWork),
				overtimeAvailabilityRepository,
				new PersistableScheduleDataPermissionChecker(authorization),
				new ScheduleStorageRepositoryWrapper(() => _personAssignmentRepository,
					() => personAbsenceRepository,
					() => preferenceDayRepository, () => noteRepository,
					() => publicNoteRepository,
					() => studentAvailabilityDayRepository,
					() => agentDayScheduleTagRepository,
					() => overtimeAvailabilityRepository), authorization);
		}
	}
}