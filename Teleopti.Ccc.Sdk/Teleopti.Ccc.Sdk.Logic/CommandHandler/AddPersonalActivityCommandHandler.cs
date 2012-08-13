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
	public class AddPersonalActivityCommandHandler : IHandleCommand<AddPersonalActivityCommandDto>
	{
		private readonly IAssembler<DateTimePeriod, DateTimePeriodDto> _dateTimePeriodAssembler;
		private readonly IActivityRepository _activityRepository;
		private readonly IScheduleRepository _scheduleRepository;
		private readonly IPersonRepository _personRepository;
		private readonly IScenarioRepository _scenarioRepository;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly ISaveSchedulePartService _saveSchedulePartService;
		private readonly IMessageBrokerEnablerFactory _messageBrokerEnablerFactory;
		private readonly IBusinessRulesForPersonalAccountUpdate _businessRulesForPersonalAccountUpdate;

		public AddPersonalActivityCommandHandler(IAssembler<DateTimePeriod, DateTimePeriodDto> dateTimePeriodAssembler, IActivityRepository activityRepository, IScheduleRepository scheduleRepository, IPersonRepository personRepository, IScenarioRepository scenarioRepository, IUnitOfWorkFactory unitOfWorkFactory, ISaveSchedulePartService saveSchedulePartService, IMessageBrokerEnablerFactory messageBrokerEnablerFactory, IBusinessRulesForPersonalAccountUpdate businessRulesForPersonalAccountUpdate)
		{
			_dateTimePeriodAssembler = dateTimePeriodAssembler;
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
		public CommandResultDto Handle(AddPersonalActivityCommandDto command)
		{
			using (var uow = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
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

				var activity = _activityRepository.Load(command.ActivityId);
				var personalShiftActivityLayer = new PersonalShiftActivityLayer(activity, _dateTimePeriodAssembler.DtoToDomainEntity(command.Period));
				scheduleDay.CreateAndAddPersonalActivity(personalShiftActivityLayer);
				_saveSchedulePartService.Save(scheduleDay, rules);
				using (_messageBrokerEnablerFactory.NewMessageBrokerEnabler())
				{
					uow.PersistAll();
				}
			}
			return new CommandResultDto { AffectedId = command.PersonId, AffectedItems = 1 };
		}

		private IScenario getDesiredScenario(AddPersonalActivityCommandDto command)
		{
			return command.ScenarioId.HasValue ? _scenarioRepository.Get(command.ScenarioId.Value) : _scenarioRepository.LoadDefaultScenario();
		}
	}
}