using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer
{
	public class RemoveAbsenceCommandHandler : IHandleCommand<RemoveAbsenceCommand>
	{
		private readonly ICurrentDataSource _currentDataSource;
		private readonly IWriteSideRepository<IPersonAbsence> _personAbsenceRepository;

		public RemoveAbsenceCommandHandler(ICurrentDataSource currentDataSource, IWriteSideRepository<IPersonAbsence> personAbsenceRepository)
		{
			_currentDataSource = currentDataSource;
			_personAbsenceRepository = personAbsenceRepository;
		}

		public void Handle(RemoveAbsenceCommand command)
		{
			var personAbsence = (PersonAbsence) _personAbsenceRepository.Load(command.PersonAbsenceId);

			personAbsence.RemoveAbsence(
				_currentDataSource.CurrentName()
				);

			_personAbsenceRepository.Remove(personAbsence);
		}
	}
}