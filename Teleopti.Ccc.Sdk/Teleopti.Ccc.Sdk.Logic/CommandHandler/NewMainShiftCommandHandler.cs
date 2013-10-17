﻿using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Ccc.Sdk.Logic.QueryHandler;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.Logic.CommandHandler
{
    public class NewMainShiftCommandHandler : IHandleCommand<NewMainShiftCommandDto>
    {
        private readonly ICurrentUnitOfWorkFactory _unitOfWorkFactory;
        private readonly IShiftCategoryRepository _shiftCategoryRepository;
        private readonly IActivityLayerAssembler<IMainShiftLayer> _mainActivityLayerAssembler;
	    private readonly IScheduleTagAssembler _scheduleTagAssembler;
	    private readonly IScheduleRepository _scheduleRepository;
        private readonly IScenarioRepository _scenarioRepository;
        private readonly IPersonRepository _personRepository;
        private readonly ISaveSchedulePartService _saveSchedulePartService;
    	private readonly IMessageBrokerEnablerFactory _messageBrokerEnablerFactory;
    	private readonly IBusinessRulesForPersonalAccountUpdate _businessRulesForPersonalAccountUpdate;

    	public NewMainShiftCommandHandler(ICurrentUnitOfWorkFactory unitOfWorkFactory,IShiftCategoryRepository shiftCategoryRepository,IActivityLayerAssembler<IMainShiftLayer> mainActivityLayerAssembler, IScheduleTagAssembler scheduleTagAssembler, IScheduleRepository scheduleRepository, IScenarioRepository scenarioRepository, IPersonRepository personRepository, ISaveSchedulePartService saveSchedulePartService, IMessageBrokerEnablerFactory messageBrokerEnablerFactory, IBusinessRulesForPersonalAccountUpdate businessRulesForPersonalAccountUpdate)
        {
            _unitOfWorkFactory = unitOfWorkFactory;
            _shiftCategoryRepository = shiftCategoryRepository;
            _mainActivityLayerAssembler = mainActivityLayerAssembler;
    		_scheduleTagAssembler = scheduleTagAssembler;
    		_scheduleRepository = scheduleRepository;
            _scenarioRepository = scenarioRepository;
            _personRepository = personRepository;
            _saveSchedulePartService = saveSchedulePartService;
        	_messageBrokerEnablerFactory = messageBrokerEnablerFactory;
    		_businessRulesForPersonalAccountUpdate = businessRulesForPersonalAccountUpdate;
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void Handle(NewMainShiftCommandDto command)
        {
            using (var uow = _unitOfWorkFactory.LoggedOnUnitOfWorkFactory().CreateAndOpenUnitOfWork())
            {
                var person = _personRepository.Load(command.PersonId);
                var scenario = getDesiredScenario(command);
                var startDate = command.Date.ToDateOnly();
				var scheduleDictionary = _scheduleRepository.FindSchedulesOnlyForGivenPeriodAndPerson(
                    person, new ScheduleDictionaryLoadOptions(false, false),
                    new DateOnlyPeriod(startDate, startDate.AddDays(1)), scenario);

				var scheduleRange = scheduleDictionary[person];
				var rules = _businessRulesForPersonalAccountUpdate.FromScheduleRange(scheduleRange);
                var scheduleDay = scheduleRange.ScheduledDay(startDate);
                var shiftCategory = _shiftCategoryRepository.Load(command.ShiftCategoryId);
	            var mainShiftLayers = _mainActivityLayerAssembler.DtosToDomainEntities(command.LayerCollection);
               
				IPersonAssignment currentAss = scheduleDay.PersonAssignment();

				if (currentAss == null)
				{
					currentAss = new PersonAssignment(scheduleDay.Person, scheduleDay.Scenario, scheduleDay.DateOnlyAsPeriod.DateOnly);
					scheduleDay.Add(currentAss);
				}
							currentAss.SetShiftCategory(shiftCategory);
	            foreach (var shiftLayer in mainShiftLayers)
	            {
		            currentAss.AddMainLayer(shiftLayer.Payload, shiftLayer.Period);
	            }

				var scheduleTagEntity = _scheduleTagAssembler.DtoToDomainEntity(new ScheduleTagDto { Id = command.ScheduleTagId });
				_saveSchedulePartService.Save(scheduleDay, rules, scheduleTagEntity);
                using (_messageBrokerEnablerFactory.NewMessageBrokerEnabler())
                {
                    uow.PersistAll();
                }
            }
			command.Result = new CommandResultDto { AffectedId = command.PersonId, AffectedItems = 1 };
        }

    	private IScenario getDesiredScenario(NewMainShiftCommandDto command)
    	{
    		return command.ScenarioId.HasValue ? _scenarioRepository.Get(command.ScenarioId.Value) : _scenarioRepository.LoadDefaultScenario();
    	}
    }
}
