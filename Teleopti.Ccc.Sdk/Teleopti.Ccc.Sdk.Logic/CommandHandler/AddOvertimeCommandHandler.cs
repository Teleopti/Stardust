﻿using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.Logic.CommandHandler
{
    public class AddOvertimeCommandHandler : IHandleCommand<AddOvertimeCommandDto>
    {
        private readonly IAssembler<DateTimePeriod, DateTimePeriodDto> _dateTimePeriodAssembler;
        private readonly IMultiplicatorDefinitionSetRepository _multiplicatorDefinitionSetRepository;
        private readonly IActivityRepository _activityRepository;
        private readonly IScheduleRepository _scheduleRepository;
        private readonly IPersonRepository _personRepository;
        private readonly IScenarioRepository _scenarioRepository;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly ISaveSchedulePartService _saveSchedulePartService;
    	private readonly IMessageBrokerEnablerFactory _messageBrokerEnablerFactory;
    	private readonly IBusinessRulesForPersonalAccountUpdate _businessRulesForPersonalAccountUpdate;

    	public AddOvertimeCommandHandler(IAssembler<DateTimePeriod, DateTimePeriodDto> dateTimePeriodAssembler, IMultiplicatorDefinitionSetRepository multiplicatorDefinitionSetRepository, IActivityRepository activityRepository, IScheduleRepository scheduleRepository, IPersonRepository personRepository, IScenarioRepository scenarioRepository, IUnitOfWorkFactory unitOfWorkFactory, ISaveSchedulePartService saveSchedulePartService, IMessageBrokerEnablerFactory messageBrokerEnablerFactory, IBusinessRulesForPersonalAccountUpdate businessRulesForPersonalAccountUpdate)
        {
            _dateTimePeriodAssembler = dateTimePeriodAssembler;
            _multiplicatorDefinitionSetRepository = multiplicatorDefinitionSetRepository;
            _activityRepository = activityRepository;
            _scheduleRepository = scheduleRepository;
            _personRepository = personRepository;
            _scenarioRepository = scenarioRepository;
            _unitOfWorkFactory = unitOfWorkFactory;
            _saveSchedulePartService = saveSchedulePartService;
        	_messageBrokerEnablerFactory = messageBrokerEnablerFactory;
    		_businessRulesForPersonalAccountUpdate = businessRulesForPersonalAccountUpdate;
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		public CommandResultDto Handle(AddOvertimeCommandDto command)
        {
            using (var uow = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
            {
                var person = _personRepository.Load(command.PersonId);
                var scenario = _scenarioRepository.LoadDefaultScenario();
                var startDate = new DateOnly(command.Date.DateTime);
                var timeZone = person.PermissionInformation.DefaultTimeZone();
                var scheduleDictionary = _scheduleRepository.FindSchedulesOnlyInGivenPeriod(
                    new PersonProvider(new[] { person }), new ScheduleDictionaryLoadOptions(false, false),
                    new DateOnlyPeriod(startDate, startDate.AddDays(1)).ToDateTimePeriod(timeZone), scenario);

				var scheduleRange = scheduleDictionary[person];
				var rules = _businessRulesForPersonalAccountUpdate.FromScheduleRange(scheduleRange);
                var scheduleDay = scheduleRange.ScheduledDay(startDate);

                var activity = _activityRepository.Load(command.ActivityId);
                var overtimeDefinition = _multiplicatorDefinitionSetRepository.Load(command.OvertimeDefinitionSetId);
                var overtimeLayer = new OvertimeShiftActivityLayer(activity, _dateTimePeriodAssembler.DtoToDomainEntity(command.Period), overtimeDefinition);
                scheduleDay.CreateAndAddOvertime(overtimeLayer);
				_saveSchedulePartService.Save(scheduleDay, rules);
                using (_messageBrokerEnablerFactory.NewMessageBrokerEnabler())
                {
                    uow.PersistAll();
                }
            }
            return new CommandResultDto { AffectedId = command.PersonId, AffectedItems = 1 };
        }
    }
}
