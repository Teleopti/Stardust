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
	public class AddPersonalActivityCommandHandler : IHandleCommand<AddPersonalActivityCommandDto>
	{
		private readonly IAssembler<DateTimePeriod, DateTimePeriodDto> _dateTimePeriodAssembler;
		private readonly IActivityRepository _activityRepository;
		private readonly IScheduleRepository _scheduleRepository;
		private readonly IPersonRepository _personRepository;
		private readonly IScenarioRepository _scenarioRepository;
        private readonly ICurrentUnitOfWorkFactory _unitOfWorkFactory;
		private readonly ISaveSchedulePartService _saveSchedulePartService;
		private readonly IBusinessRulesForPersonalAccountUpdate _businessRulesForPersonalAccountUpdate;
		private readonly IScheduleTagAssembler _scheduleTagAssembler;

		public AddPersonalActivityCommandHandler(IAssembler<DateTimePeriod, DateTimePeriodDto> dateTimePeriodAssembler, IActivityRepository activityRepository, IScheduleRepository scheduleRepository, IPersonRepository personRepository, IScenarioRepository scenarioRepository, ICurrentUnitOfWorkFactory unitOfWorkFactory, ISaveSchedulePartService saveSchedulePartService, IBusinessRulesForPersonalAccountUpdate businessRulesForPersonalAccountUpdate, IScheduleTagAssembler scheduleTagAssembler)
		{
			_dateTimePeriodAssembler = dateTimePeriodAssembler;
			_activityRepository = activityRepository;
			_scheduleRepository = scheduleRepository;
			_personRepository = personRepository;
			_scenarioRepository = scenarioRepository;
			_unitOfWorkFactory = unitOfWorkFactory;
			_saveSchedulePartService = saveSchedulePartService;
			_businessRulesForPersonalAccountUpdate = businessRulesForPersonalAccountUpdate;
			_scheduleTagAssembler = scheduleTagAssembler;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		public void Handle(AddPersonalActivityCommandDto command)
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

				var activity = _activityRepository.Load(command.ActivityId);
				scheduleDay.CreateAndAddPersonalActivity(activity, _dateTimePeriodAssembler.DtoToDomainEntity(command.Period));

				var scheduleTagEntity = _scheduleTagAssembler.DtoToDomainEntity(new ScheduleTagDto { Id = command.ScheduleTagId });
				_saveSchedulePartService.Save(scheduleDay, rules, scheduleTagEntity);
				uow.PersistAll();
			}
			command.Result = new CommandResultDto { AffectedId = command.PersonId, AffectedItems = 1 };
		}

		private IScenario getDesiredScenario(AddPersonalActivityCommandDto command)
		{
			return command.ScenarioId.HasValue ? _scenarioRepository.Get(command.ScenarioId.Value) : _scenarioRepository.LoadDefaultScenario();
		}
	}
}