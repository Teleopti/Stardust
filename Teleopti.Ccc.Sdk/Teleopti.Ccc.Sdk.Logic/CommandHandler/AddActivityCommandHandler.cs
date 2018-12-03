using System.ServiceModel;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Ccc.Sdk.Logic.QueryHandler;

namespace Teleopti.Ccc.Sdk.Logic.CommandHandler
{
    public class AddActivityCommandHandler :IHandleCommand<AddActivityCommandDto>
    {
        private readonly IAssembler<DateTimePeriod, DateTimePeriodDto> _dateTimePeriodAssembler;
        private readonly IActivityRepository _activityRepository;
        private readonly IScheduleStorage _scheduleStorage;
        private readonly IPersonRepository _personRepository;
        private readonly IScenarioRepository _scenarioRepository;
        private readonly ICurrentUnitOfWorkFactory _unitOfWorkFactory;
    	private readonly IBusinessRulesForPersonalAccountUpdate _businessRulesForPersonalAccountUpdate;
		private readonly IScheduleTagAssembler _scheduleTagAssembler;
		private readonly IScheduleSaveHandler _scheduleSaveHandler;
		
		public AddActivityCommandHandler(IAssembler<DateTimePeriod, DateTimePeriodDto> dateTimePeriodAssembler, IActivityRepository activityRepository, IScheduleStorage scheduleStorage, IPersonRepository personRepository, IScenarioRepository scenarioRepository, ICurrentUnitOfWorkFactory unitOfWorkFactory, IBusinessRulesForPersonalAccountUpdate businessRulesForPersonalAccountUpdate, IScheduleTagAssembler scheduleTagAssembler, IScheduleSaveHandler scheduleSaveHandler)
        {
            _dateTimePeriodAssembler = dateTimePeriodAssembler;
            _activityRepository = activityRepository;
            _scheduleStorage = scheduleStorage;
            _personRepository = personRepository;
            _scenarioRepository = scenarioRepository;
            _unitOfWorkFactory = unitOfWorkFactory;
    		_businessRulesForPersonalAccountUpdate = businessRulesForPersonalAccountUpdate;
			_scheduleTagAssembler = scheduleTagAssembler;
			_scheduleSaveHandler = scheduleSaveHandler;
        }
		
	    public void Handle(AddActivityCommandDto command)
	    {
		    using (var uow = _unitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
		    {
			    var person = _personRepository.Load(command.PersonId);
			    var scenario = getDesiredScenario(command);
			    var startDate = command.Date.ToDateOnly();
			    var scheduleDictionary = _scheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(
				    person, new ScheduleDictionaryLoadOptions(false, false),
				    new DateOnlyPeriod(startDate, startDate.AddDays(1)), scenario);

			    var scheduleRange = scheduleDictionary[person];
			    var rules = _businessRulesForPersonalAccountUpdate.FromScheduleRange(scheduleRange);
			    var scheduleDay = scheduleRange.ScheduledDay(startDate);

			    IShiftCategory shiftCategory;
			    var personAssignment = scheduleDay.PersonAssignment();
			    if (personAssignment != null)
			    {
				    shiftCategory = personAssignment.ShiftCategory;
			    }
			    else
				    throw new FaultException("A main shift should exist first before you add a new activity.");

			    var activity = _activityRepository.Load(command.ActivityId);
			    scheduleDay.CreateAndAddActivity(activity, _dateTimePeriodAssembler.DtoToDomainEntity(command.Period),
				    shiftCategory);

			    var scheduleTagEntity = _scheduleTagAssembler.DtoToDomainEntity(new ScheduleTagDto {Id = command.ScheduleTagId});

			    _scheduleSaveHandler.ProcessSave(scheduleDay, rules, scheduleTagEntity);
			    uow.PersistAll();

		    }
		    command.Result = new CommandResultDto {AffectedId = command.PersonId, AffectedItems = 1};
	    }

	    private IScenario getDesiredScenario(AddActivityCommandDto command)
    	{
    		return  command.ScenarioId.HasValue ? _scenarioRepository.Get(command.ScenarioId.Value) : _scenarioRepository.LoadDefaultScenario();
    	}
    }
}
