using System.ServiceModel;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Ccc.Sdk.WcfService.Factory;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.WcfService.CommandHandler
{
    public class CancelAbsenceCommandHandler : IHandleCommand<CancelAbsenceCommandDto>
    {
        private readonly IAssembler<DateTimePeriod, DateTimePeriodDto> _dateTimePeriodAssembler;
        private readonly IScheduleRepository _scheduleRepository;
        private readonly IPersonRepository _personRepository;
        private readonly IScenarioRepository _scenarioRepository;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly ISaveSchedulePartService _saveSchedulePartService;

        public CancelAbsenceCommandHandler(IAssembler<DateTimePeriod, DateTimePeriodDto> dateTimePeriodAssembler, IScheduleRepository scheduleRepository, IPersonRepository personRepository, IScenarioRepository scenarioRepository, IUnitOfWorkFactory unitOfWorkFactory, ISaveSchedulePartService saveSchedulePartService)
        {
            _dateTimePeriodAssembler = dateTimePeriodAssembler;
            _scheduleRepository = scheduleRepository;
            _personRepository = personRepository;
            _scenarioRepository = scenarioRepository;
            _unitOfWorkFactory = unitOfWorkFactory;
            _saveSchedulePartService = saveSchedulePartService;
        }

        public CommandResultDto Handle(CancelAbsenceCommandDto command)
        {
			using (var uow = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
			{
				var person = _personRepository.Load(command.PersonId);
				var scenario = _scenarioRepository.LoadDefaultScenario();

				var dateTimePeriod = _dateTimePeriodAssembler.DtoToDomainEntity(command.Period);
				var timeZone = person.PermissionInformation.DefaultTimeZone();
				var startDate = new DateOnly(dateTimePeriod.StartDateTimeLocal(timeZone));
				var endDate = startDate.AddDays(1);

				var scheduleDictionary =
					_scheduleRepository.FindSchedulesOnlyInGivenPeriod(
						new PersonProvider(new[] {person}), new ScheduleDictionaryLoadOptions(false, false),
						new DateOnlyPeriod(startDate, endDate).ToDateTimePeriod(timeZone), scenario);

				ExtractedSchedule scheduleDay = scheduleDictionary[person].ScheduledDay(startDate) as ExtractedSchedule;
				if (scheduleDay == null) throw new FaultException("This is not a valid day to perform this action for.");
				var absences = scheduleDay.PersonAbsenceCollection(true);
				foreach (var personAbsence in absences)
				{
					var splitPeriods = personAbsence.Split(dateTimePeriod);
					if (splitPeriods.Count > 0 || dateTimePeriod.Contains(personAbsence.Layer.Period))
					{
						scheduleDay.Remove(personAbsence);
						foreach (var splitPeriod in splitPeriods)
						{
							scheduleDay.Add(splitPeriod);
						}
					}
				}

				_saveSchedulePartService.Save(uow, scheduleDay);
				using (new MessageBrokerSendEnabler())
				{
					uow.PersistAll();
				}
			}
        	return new CommandResultDto {AffectedId = command.PersonId, AffectedItems = 1};
        }
    }
}
