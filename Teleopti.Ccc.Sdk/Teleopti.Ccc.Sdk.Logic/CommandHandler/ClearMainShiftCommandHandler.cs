using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Ccc.Sdk.Logic.QueryHandler;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.Logic.CommandHandler
{
    public class ClearMainShiftCommandHandler : IHandleCommand<ClearMainShiftCommandDto>
    {
	    private readonly IScheduleTagAssembler _scheduleTagAssembler;
	    private readonly IScheduleRepository _scheduleRepository;
        private readonly IPersonRepository _personRepository;
        private readonly IScenarioRepository _scenarioRepository;
        private readonly ICurrentUnitOfWorkFactory _unitOfWorkFactory;
        private readonly ISaveSchedulePartService _saveSchedulePartService;
    	private readonly IBusinessRulesForPersonalAccountUpdate _businessRulesForPersonalAccountUpdate;

    	public ClearMainShiftCommandHandler(IScheduleTagAssembler scheduleTagAssembler, IScheduleRepository scheduleRepository, IPersonRepository personRepository, IScenarioRepository scenarioRepository, ICurrentUnitOfWorkFactory unitOfWorkFactory, ISaveSchedulePartService saveSchedulePartService, IBusinessRulesForPersonalAccountUpdate businessRulesForPersonalAccountUpdate)
        {
    		_scheduleTagAssembler = scheduleTagAssembler;
    		_scheduleRepository = scheduleRepository;
            _personRepository = personRepository;
            _scenarioRepository = scenarioRepository;
            _unitOfWorkFactory = unitOfWorkFactory;
            _saveSchedulePartService = saveSchedulePartService;
    		_businessRulesForPersonalAccountUpdate = businessRulesForPersonalAccountUpdate;
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void Handle(ClearMainShiftCommandDto command)
        {
            using (var uow = _unitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
            {
                var person = _personRepository.Load(command.PersonId);
                var scenario = getDesiredScenario(command);
                var startDate = command.Date.ToDateOnly();
	            var scheduleDictionary = _scheduleRepository.FindSchedulesForPersonOnlyInGivenPeriod(
		            person, new ScheduleDictionaryLoadOptions(false, false),
		            new DateOnlyPeriod(startDate, startDate.AddDays(1)), scenario);

				var scheduleRange = scheduleDictionary[person];
				var rules = _businessRulesForPersonalAccountUpdate.FromScheduleRange(scheduleRange);
                var scheduleDay = scheduleRange.ScheduledDay(startDate);
                scheduleDay.DeleteMainShift(scheduleDay);
				
				var scheduleTagEntity = _scheduleTagAssembler.DtoToDomainEntity(new ScheduleTagDto { Id = command.ScheduleTagId });
				_saveSchedulePartService.Save(scheduleDay, rules, scheduleTagEntity);
                    uow.PersistAll();
            }
			command.Result = new CommandResultDto { AffectedId = command.PersonId, AffectedItems = 1 };
        }

    	private IScenario getDesiredScenario(ClearMainShiftCommandDto command)
    	{
    		return command.ScenarioId.HasValue ? _scenarioRepository.Get(command.ScenarioId.Value) : _scenarioRepository.LoadDefaultScenario();
    	}
    }
}
