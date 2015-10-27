using System.ServiceModel;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.Logic.CommandHandler
{
    public class CancelAbsenceCommandHandler : IHandleCommand<CancelAbsenceCommandDto>
    {
        private readonly IAssembler<DateTimePeriod, DateTimePeriodDto> _dateTimePeriodAssembler;
	    private readonly IScheduleTagAssembler _scheduleTagAssembler;
	    private readonly IScheduleRepository _scheduleRepository;
        private readonly IPersonRepository _personRepository;
        private readonly IScenarioRepository _scenarioRepository;
        private readonly ICurrentUnitOfWorkFactory _unitOfWorkFactory;
        private readonly ISaveSchedulePartService _saveSchedulePartService;
    	private readonly IBusinessRulesForPersonalAccountUpdate _businessRulesForPersonalAccountUpdate;


        public CancelAbsenceCommandHandler(IAssembler<DateTimePeriod, DateTimePeriodDto> dateTimePeriodAssembler, IScheduleTagAssembler scheduleTagAssembler, IScheduleRepository scheduleRepository, IPersonRepository personRepository, IScenarioRepository scenarioRepository, ICurrentUnitOfWorkFactory unitOfWorkFactory, ISaveSchedulePartService saveSchedulePartService, IBusinessRulesForPersonalAccountUpdate businessRulesForPersonalAccountUpdate)
        {
            _dateTimePeriodAssembler = dateTimePeriodAssembler;
	        _scheduleTagAssembler = scheduleTagAssembler;
	        _scheduleRepository = scheduleRepository;
            _personRepository = personRepository;
            _scenarioRepository = scenarioRepository;
            _unitOfWorkFactory = unitOfWorkFactory;
            _saveSchedulePartService = saveSchedulePartService;
    		_businessRulesForPersonalAccountUpdate = businessRulesForPersonalAccountUpdate;
        }

		public void Handle(CancelAbsenceCommandDto command)
        {
			using (var uow = _unitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				var person = _personRepository.Load(command.PersonId);
				var scenario = getDesiredScenario(command);

				var dateTimePeriod = _dateTimePeriodAssembler.DtoToDomainEntity(command.Period);
				var timeZone = person.PermissionInformation.DefaultTimeZone();
				var startDate = new DateOnly(dateTimePeriod.StartDateTimeLocal(timeZone));
				var endDate = startDate.AddDays(1);

				var scheduleDictionary =
					_scheduleRepository.FindSchedulesForPersonOnlyInGivenPeriod(
						person, new ScheduleDictionaryLoadOptions(false, false),
						new DateOnlyPeriod(startDate, endDate), scenario);

				var scheduleRange = scheduleDictionary[person];
				var rules = _businessRulesForPersonalAccountUpdate.FromScheduleRange(scheduleRange);

				var scheduleDay = scheduleRange.ScheduledDay(startDate) as ExtractedSchedule;
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

				var scheduleTagEntity = _scheduleTagAssembler.DtoToDomainEntity(new ScheduleTagDto {Id = command.ScheduleTagId});

				try
				{
					_saveSchedulePartService.Save(scheduleDay, rules, scheduleTagEntity);
					uow.PersistAll();
				}
				catch (BusinessRuleValidationException ex)
				{
					throw new FaultException(ex.Message);
				}
			}
			command.Result = new CommandResultDto { AffectedId = command.PersonId, AffectedItems = 1 };
        }

    	private IScenario getDesiredScenario(CancelAbsenceCommandDto command)
    	{
    		return command.ScenarioId.HasValue ? _scenarioRepository.Get(command.ScenarioId.Value) : _scenarioRepository.LoadDefaultScenario();
    	}
    }
}
