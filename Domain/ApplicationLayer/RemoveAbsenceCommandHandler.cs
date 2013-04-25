using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer
{
	public class RemoveAbsenceCommandHandler : IHandleCommand<RemoveAbsenceCommand>
	{
		private readonly IWriteSideRepository<IPersonAbsence> _personAbsenceRepository;

		public RemoveAbsenceCommandHandler(IWriteSideRepository<IPersonAbsence> personAbsenceRepository)
		{
			_personAbsenceRepository = personAbsenceRepository;
		}

		public void Handle(RemoveAbsenceCommand command)
		{
			var personAbsence = _personAbsenceRepository.Load(command.PersonAbsenceId);

			_personAbsenceRepository.Remove(personAbsence);
		}
	}
}