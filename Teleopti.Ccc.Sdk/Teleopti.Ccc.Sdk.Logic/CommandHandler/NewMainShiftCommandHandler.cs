using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
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
    public class NewMainShiftCommandHandler : IHandleCommand<NewMainShiftCommandDto>
    {
        private readonly ICurrentUnitOfWorkFactory _unitOfWorkFactory;
        private readonly IShiftCategoryRepository _shiftCategoryRepository;
        private readonly IActivityLayerAssembler<IMainShiftActivityLayer> _mainActivityLayerAssembler;
        private readonly IScheduleRepository _scheduleRepository;
        private readonly IScenarioRepository _scenarioRepository;
        private readonly IPersonRepository _personRepository;
        private readonly ISaveSchedulePartService _saveSchedulePartService;
    	private readonly IMessageBrokerEnablerFactory _messageBrokerEnablerFactory;
    	private readonly IBusinessRulesForPersonalAccountUpdate _businessRulesForPersonalAccountUpdate;

    	public NewMainShiftCommandHandler(ICurrentUnitOfWorkFactory unitOfWorkFactory,IShiftCategoryRepository shiftCategoryRepository,IActivityLayerAssembler<IMainShiftActivityLayer> mainActivityLayerAssembler, IScheduleRepository scheduleRepository, IScenarioRepository scenarioRepository, IPersonRepository personRepository, ISaveSchedulePartService saveSchedulePartService, IMessageBrokerEnablerFactory messageBrokerEnablerFactory, IBusinessRulesForPersonalAccountUpdate businessRulesForPersonalAccountUpdate)
        {
            _unitOfWorkFactory = unitOfWorkFactory;
            _shiftCategoryRepository = shiftCategoryRepository;
            _mainActivityLayerAssembler = mainActivityLayerAssembler;
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
                var startDate = new DateOnly(command.Date.DateTime);
                var timeZone = person.PermissionInformation.DefaultTimeZone();
                var scheduleDictionary = _scheduleRepository.FindSchedulesOnlyInGivenPeriod(
                    new PersonProvider(new[] { person }), new ScheduleDictionaryLoadOptions(false, false),
                    new DateOnlyPeriod(startDate, startDate.AddDays(1)).ToDateTimePeriod(timeZone), scenario);

				var scheduleRange = scheduleDictionary[person];
				var rules = _businessRulesForPersonalAccountUpdate.FromScheduleRange(scheduleRange);
                var scheduleDay = scheduleRange.ScheduledDay(startDate);
                var shiftCategory = _shiftCategoryRepository.Load(command.ShiftCategoryId);
                var mainShift = new MainShift(shiftCategory);
                addLayersToMainShift(mainShift, command.LayerCollection);
				IPersonAssignment currentAss = scheduleDay.AssignmentHighZOrder();
				scheduleDay.MergePersonalShiftsToOneAssignment(mainShift.LayerCollection.Period().Value);
				if (currentAss == null)
				{
					currentAss = new PersonAssignment(scheduleDay.Person, scheduleDay.Scenario, scheduleDay.DateOnlyAsPeriod.DateOnly);
					scheduleDay.Add(currentAss);
				}
				// use scheduleDay.AddMainShift again when MainShift is removed
				var layerList = new List<IMainShiftActivityLayerNew>();
				foreach (var layer in mainShift.LayerCollection)
				{
					layerList.Add(new MainShiftActivityLayerNew(layer.Payload, layer.Period));
				}
				currentAss.SetMainShiftLayers(layerList, mainShift.ShiftCategory);
				_saveSchedulePartService.Save(scheduleDay, rules);
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

    	private void addLayersToMainShift(IMainShift mainShift, IEnumerable<ActivityLayerDto> layerDtos)
        {
            _mainActivityLayerAssembler.DtosToDomainEntities(layerDtos).ForEach(mainShift.LayerCollection.Add);
        }
    }
}
