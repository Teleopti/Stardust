using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public class AddIntradayAbsenceCommandHandler : IHandleCommand<AddIntradayAbsenceCommand>
	{
		private readonly IScheduleRepository _scheduleRepository;
		private readonly IProxyForId<IPerson> _personRepository;
		private readonly IProxyForId<IAbsence> _absenceRepository;
		private readonly IWriteSideRepository<IPersonAbsence> _personAbsenceRepository;
		private readonly ICurrentScenario _scenario;
		private readonly IUserTimeZone _timeZone;

		public AddIntradayAbsenceCommandHandler(IScheduleRepository scheduleRepository, IProxyForId<IPerson> personRepository,
		                                        IProxyForId<IAbsence> absenceRepository, IWriteSideRepository<IPersonAbsence> personAbsenceRepository, 
		                                        ICurrentScenario scenario, IUserTimeZone timeZone)
		{
			_scheduleRepository = scheduleRepository;
			_personRepository = personRepository;
			_absenceRepository = absenceRepository;
			_personAbsenceRepository = personAbsenceRepository;
			_scenario = scenario;
			_timeZone = timeZone;
		}

		public void Handle(AddIntradayAbsenceCommand command)
		{
			var person = _personRepository.Load(command.PersonId);
			var absence = _absenceRepository.Load(command.AbsenceId);
			var absenceTimePeriod = new DateTimePeriod(TimeZoneHelper.ConvertToUtc(command.StartDateTime, _timeZone.TimeZone()),
			                                           TimeZoneHelper.ConvertToUtc(command.EndDateTime, _timeZone.TimeZone()));

			var personAbsence = new PersonAbsence(_scenario.Current());
			personAbsence.IntradayAbsence(person, absence,
			                              absenceTimePeriod.StartDateTime,
			                              absenceTimePeriod.EndDateTime);
			_personAbsenceRepository.Add(personAbsence);
		}
	}
}