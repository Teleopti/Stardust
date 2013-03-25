using System;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer
{
	public class AddFullDayAbsenceCommandHandler : IHandleCommand<AddFullDayAbsenceCommand>
	{
		private readonly ICurrentScenario _scenario;
		private readonly IWriteSideRepository<IPerson> _personRepository;
		private readonly IWriteSideRepository<IAbsence> _absenceRepository;
		private readonly IWriteSideRepository<IPersonAbsence> _personAbsenceRepository;

		public AddFullDayAbsenceCommandHandler(ICurrentScenario scenario, IWriteSideRepository<IPerson> personRepository, IWriteSideRepository<IAbsence> absenceRepository, IWriteSideRepository<IPersonAbsence> personAbsenceRepository)
		{
			_scenario = scenario;
			_personRepository = personRepository;
			_absenceRepository = absenceRepository;
			_personAbsenceRepository = personAbsenceRepository;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void Handle(AddFullDayAbsenceCommand command)
		{
			var person = _personRepository.Get(command.PersonId);
			var absence = _absenceRepository.Get(command.AbsenceId);

			var personAbsence = new PersonAbsence(_scenario.Current());
			personAbsence.FullDayAbsence(person, absence, command.StartDate, command.EndDate);

			_personAbsenceRepository.Add(personAbsence);

		}
	}
}