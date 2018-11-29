using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Ccc.Sdk.Logic.QueryHandler;

namespace Teleopti.Ccc.Sdk.Logic.CommandHandler
{
    public class NewMainShiftCommandHandler : IHandleCommand<NewMainShiftCommandDto>
    {
        private readonly ICurrentUnitOfWorkFactory _unitOfWorkFactory;
        private readonly IShiftCategoryRepository _shiftCategoryRepository;
        private readonly IActivityLayerAssembler<MainShiftLayer> _mainActivityLayerAssembler;
	    private readonly IScheduleTagAssembler _scheduleTagAssembler;
	    private readonly IScheduleStorage _scheduleStorage;
        private readonly IScenarioRepository _scenarioRepository;
        private readonly IPersonRepository _personRepository;
    	private readonly IBusinessRulesForPersonalAccountUpdate _businessRulesForPersonalAccountUpdate;
		private readonly IScheduleSaveHandler _scheduleSaveHandler;

    	public NewMainShiftCommandHandler(ICurrentUnitOfWorkFactory unitOfWorkFactory,IShiftCategoryRepository shiftCategoryRepository,IActivityLayerAssembler<MainShiftLayer> mainActivityLayerAssembler, IScheduleTagAssembler scheduleTagAssembler, IScheduleStorage scheduleStorage, IScenarioRepository scenarioRepository, IPersonRepository personRepository, IBusinessRulesForPersonalAccountUpdate businessRulesForPersonalAccountUpdate, IScheduleSaveHandler scheduleSaveHandler)
        {
            _unitOfWorkFactory = unitOfWorkFactory;
            _shiftCategoryRepository = shiftCategoryRepository;
            _mainActivityLayerAssembler = mainActivityLayerAssembler;
    		_scheduleTagAssembler = scheduleTagAssembler;
    		_scheduleStorage = scheduleStorage;
            _scenarioRepository = scenarioRepository;
            _personRepository = personRepository;
    		_businessRulesForPersonalAccountUpdate = businessRulesForPersonalAccountUpdate;
    		_scheduleSaveHandler = scheduleSaveHandler;
        }
		
	    public void Handle(NewMainShiftCommandDto command)
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
			    var shiftCategory = _shiftCategoryRepository.Load(command.ShiftCategoryId);
			    var mainShiftLayers = _mainActivityLayerAssembler.DtosToDomainEntities(command.LayerCollection);

			    IPersonAssignment currentAss = scheduleDay.PersonAssignment();

			    if (currentAss == null)
			    {
				    currentAss = new PersonAssignment(scheduleDay.Person, scheduleDay.Scenario,
					    scheduleDay.DateOnlyAsPeriod.DateOnly);
				    scheduleDay.Add(currentAss);
			    }
			    currentAss.SetShiftCategory(shiftCategory);
			    foreach (var shiftLayer in mainShiftLayers)
			    {
				    currentAss.AddActivity(shiftLayer.Payload, shiftLayer.Period);
			    }

			    var scheduleTagEntity = _scheduleTagAssembler.DtoToDomainEntity(new ScheduleTagDto {Id = command.ScheduleTagId});

			    _scheduleSaveHandler.ProcessSave(scheduleDay, rules, scheduleTagEntity);
			    uow.PersistAll();
		    }
		    command.Result = new CommandResultDto {AffectedId = command.PersonId, AffectedItems = 1};
	    }

	    private IScenario getDesiredScenario(NewMainShiftCommandDto command)
    	{
    		return command.ScenarioId.HasValue ? _scenarioRepository.Get(command.ScenarioId.Value) : _scenarioRepository.LoadDefaultScenario();
    	}
    }
}
