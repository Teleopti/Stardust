using System;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer
{
	public class AddFullDayAbsenceCommandHandler : IHandleCommand<AddFullDayAbsenceCommand>
	{
		//private readonly IPersonRepository _personRepository;
		//private readonly IScenarioRepository _scenarioRepository;
		//private readonly IAbsenceRepository _absenceRepository;
		//private readonly IPersonAbsenceRepository _personAbsenceRepository;

		//public AddFullDayAbsenceCommandHandler(IPersonRepository personRepository, IScenarioRepository scenarioRepository, IAbsenceRepository absenceRepository, IPersonAbsenceRepository personAbsenceRepository)
		//{
		//	_personRepository = personRepository;
		//	_scenarioRepository = scenarioRepository;
		//	_absenceRepository = absenceRepository;
		//	_personAbsenceRepository = personAbsenceRepository;
		//}

		public void Handle(AddFullDayAbsenceCommand command)
		{
			//var person = _personRepository.Get(command.PersonId);
			//var absence = _absenceRepository.Get(command.AbsenceId);

			//var startDate = TimeZoneInfo.ConvertTime(command.StartDate, person.PermissionInformation.DefaultTimeZone(), TimeZoneInfo.Utc);
			//var endDate = TimeZoneInfo.ConvertTime(command.EndDate.AddDays(1), person.PermissionInformation.DefaultTimeZone(), TimeZoneInfo.Utc);

			//var absenceLayer = new AbsenceLayer(absence, new DateTimePeriod(startDate, endDate));
			//var personAbsence = new PersonAbsence(person, _scenarioRepository.LoadDefaultScenario(), absenceLayer);
			//_personAbsenceRepository.Add(personAbsence);
		}
	}
}