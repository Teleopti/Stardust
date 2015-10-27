using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Interfaces.Domain;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public class AddIntradayAbsenceCommandHandler : IHandleCommand<AddIntradayAbsenceCommand>
	{
		private readonly IProxyForId<IPerson> _personRepository;
		private readonly IProxyForId<IAbsence> _absenceRepository;
		private readonly IScheduleRepository _scheduleRepository;
		private readonly ICurrentScenario _scenario;
		private readonly IUserTimeZone _timeZone;
		private readonly IPersonAbsenceCreator _personAbsenceCreator;

		public AddIntradayAbsenceCommandHandler(IProxyForId<IPerson> personRepository,
												IProxyForId<IAbsence> absenceRepository,
												IScheduleRepository scheduleRepository,
												ICurrentScenario scenario,
												IUserTimeZone timeZone,
												IPersonAbsenceCreator personAbsenceCreator)
		{
			_personRepository = personRepository;
			_absenceRepository = absenceRepository;
			_scheduleRepository = scheduleRepository;
			_scenario = scenario;
			_timeZone = timeZone;
			_personAbsenceCreator = personAbsenceCreator;
		}
		
		public void Handle(AddIntradayAbsenceCommand command)
		{
			var person = _personRepository.Load(command.PersonId);
			var absence = _absenceRepository.Load(command.AbsenceId);
			var absenceTimePeriod = new DateTimePeriod(TimeZoneHelper.ConvertToUtc(command.StartTime, _timeZone.TimeZone()),
													   TimeZoneHelper.ConvertToUtc(command.EndTime, _timeZone.TimeZone()));

			var scheduleRange = getScheduleRangeForPeriod(absenceTimePeriod.ToDateOnlyPeriod(_timeZone.TimeZone()), person);
			var scheduleDay = scheduleRange.ScheduledDay(new DateOnly(command.StartTime));

			_personAbsenceCreator.Create(absence, scheduleRange, scheduleDay, absenceTimePeriod, person, command.TrackedCommandInfo, false);
			
		}

		private IScheduleRange getScheduleRangeForPeriod(DateOnlyPeriod period, IPerson person)
		{
			var dictionary = _scheduleRepository.FindSchedulesForPersonOnlyInGivenPeriod(
					person,
					new ScheduleDictionaryLoadOptions(false, false),
					period,
					_scenario.Current());

			return dictionary[person];
		}

		
	}
}