using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer
{
	public class AddFullDayAbsenceCommandHandler : IHandleCommand<AddFullDayAbsenceCommand>
	{
		private readonly ICurrentDataSource _dataSource;
		private readonly ICurrentScenario _scenario;
		private readonly IWriteSideRepository<IPerson> _personRepository;
		private readonly IWriteSideRepository<IAbsence> _absenceRepository;
		private readonly IWriteSideRepository<IPersonAbsence> _personAbsenceRepository;

		public AddFullDayAbsenceCommandHandler(ICurrentDataSource dataSource, ICurrentScenario scenario, IWriteSideRepository<IPerson> personRepository, IWriteSideRepository<IAbsence> absenceRepository, IWriteSideRepository<IPersonAbsence> personAbsenceRepository)
		{
			_dataSource = dataSource;
			_scenario = scenario;
			_personRepository = personRepository;
			_absenceRepository = absenceRepository;
			_personAbsenceRepository = personAbsenceRepository;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void Handle(AddFullDayAbsenceCommand command)
		{
			var person = _personRepository.Load(command.PersonId);
			var absence = _absenceRepository.Load(command.AbsenceId);

			var personAbsence = new PersonAbsence(_scenario.Current());
			personAbsence.FullDayAbsence(_dataSource.CurrentName(), person, absence, command.StartDate, command.EndDate);

			_personAbsenceRepository.Add(personAbsence);

		}
	}
}