using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public class RemovePersonAbsenceCommandHandler : IHandleCommand<RemovePersonAbsenceCommand>
	{
		private readonly ICurrentDataSource _currentDataSource;
		private readonly IWriteSideRepository<IPersonAbsence> _personAbsenceRepository;

		public RemovePersonAbsenceCommandHandler(ICurrentDataSource currentDataSource, IWriteSideRepository<IPersonAbsence> personAbsenceRepository)
		{
			_currentDataSource = currentDataSource;
			_personAbsenceRepository = personAbsenceRepository;
		}

		public void Handle(RemovePersonAbsenceCommand command)
		{
			var personAbsence = (PersonAbsence) _personAbsenceRepository.Load(command.PersonAbsenceId);

			personAbsence.RemovePersonAbsence(
				_currentDataSource.CurrentName()
				);

			_personAbsenceRepository.Remove(personAbsence);
		}
	}
}