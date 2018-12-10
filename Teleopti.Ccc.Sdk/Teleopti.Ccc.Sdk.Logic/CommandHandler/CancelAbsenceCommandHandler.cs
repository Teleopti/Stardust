using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Ccc.Sdk.Logic.Assemblers;

namespace Teleopti.Ccc.Sdk.Logic.CommandHandler
{
    public class CancelAbsenceCommandHandler : IHandleCommand<CancelAbsenceCommandDto>
    {
        private readonly IAssembler<DateTimePeriod, DateTimePeriodDto> _dateTimePeriodAssembler;
	    private readonly IScheduleTagAssembler _scheduleTagAssembler;
	    private readonly IScheduleStorage _scheduleStorage;
        private readonly IPersonRepository _personRepository;
        private readonly IScenarioRepository _scenarioRepository;
        private readonly ICurrentUnitOfWorkFactory _unitOfWorkFactory;
    	private readonly IBusinessRulesForPersonalAccountUpdate _businessRulesForPersonalAccountUpdate;
		private readonly IScheduleSaveHandler _scheduleSaveHandler;

        public CancelAbsenceCommandHandler(IAssembler<DateTimePeriod, DateTimePeriodDto> dateTimePeriodAssembler, IScheduleTagAssembler scheduleTagAssembler, IScheduleStorage scheduleStorage, IPersonRepository personRepository, IScenarioRepository scenarioRepository, ICurrentUnitOfWorkFactory unitOfWorkFactory, IBusinessRulesForPersonalAccountUpdate businessRulesForPersonalAccountUpdate, IScheduleSaveHandler scheduleSaveHandler)
        {
            _dateTimePeriodAssembler = dateTimePeriodAssembler;
	        _scheduleTagAssembler = scheduleTagAssembler;
	        _scheduleStorage = scheduleStorage;
            _personRepository = personRepository;
            _scenarioRepository = scenarioRepository;
            _unitOfWorkFactory = unitOfWorkFactory;
    		_businessRulesForPersonalAccountUpdate = businessRulesForPersonalAccountUpdate;
	        _scheduleSaveHandler = scheduleSaveHandler;
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
				
				IAbsence absence = null;
				if (command.AbsenceId.HasValue)
				{
					absence = new Absence();
					absence.SetId(command.AbsenceId.Value);
				}
				var scheduleDictionary = _scheduleStorage.ScheduleRangeBasedOnAbsence(dateTimePeriod, scenario, person, absence).Owner;

				var scheduleRange = scheduleDictionary[person];
				var rules = _businessRulesForPersonalAccountUpdate.FromScheduleRange(scheduleRange);

				var scheduleDay = scheduleRange.ScheduledDay(startDate);
				var absences = scheduleDay.PersonAbsenceCollection(true);
				foreach (var personAbsence in absences)
				{
					if (command.AbsenceId.HasValue)
					{
						if (personAbsence.Layer.Payload.Id != command.AbsenceId)
							continue;
					}
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

				_scheduleSaveHandler.ProcessSave(scheduleDay, rules, scheduleTagEntity);
				uow.PersistAll();
			}
			command.Result = new CommandResultDto { AffectedId = command.PersonId, AffectedItems = 1 };
        }

    	private IScenario getDesiredScenario(CancelAbsenceCommandDto command)
    	{
    		return command.ScenarioId.HasValue ? _scenarioRepository.Get(command.ScenarioId.Value) : _scenarioRepository.LoadDefaultScenario();
    	}
    }
}
