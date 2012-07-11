using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.Logic.CommandHandler
{
    public class AddDayOffCommandHandler : IHandleCommand<AddDayOffCommandDto>
    {
        private readonly IDayOffRepository _dayOffRepository;
        private readonly IScheduleRepository _scheduleRepository;
        private readonly IPersonRepository _personRepository;
        private readonly IScenarioRepository _scenarioRepository;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly ISaveSchedulePartService _saveSchedulePartService;
    	private readonly IMessageBrokerEnablerFactory _messageBrokerEnablerFactory;

    	public AddDayOffCommandHandler(IDayOffRepository dayOffRepository, IScheduleRepository scheduleRepository, IPersonRepository personRepository, IScenarioRepository scenarioRepository, IUnitOfWorkFactory unitOfWorkFactory, ISaveSchedulePartService saveSchedulePartService, IMessageBrokerEnablerFactory messageBrokerEnablerFactory)
        {
            _dayOffRepository = dayOffRepository;
            _scheduleRepository = scheduleRepository;
            _personRepository = personRepository;
            _scenarioRepository = scenarioRepository;
            _unitOfWorkFactory = unitOfWorkFactory;
            _saveSchedulePartService = saveSchedulePartService;
        	_messageBrokerEnablerFactory = messageBrokerEnablerFactory;
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public CommandResultDto Handle(AddDayOffCommandDto command)
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
                var scheduleDay = scheduleDictionary[person].ScheduledDay(startDate);
                var dayOff = _dayOffRepository.Load(command.DayOffInfoId);
                
                scheduleDay.DeleteMainShift(scheduleDay);
                scheduleDay.DeleteOvertime();
                scheduleDay.DeleteAbsence(false);
                scheduleDay.DeleteFullDayAbsence(scheduleDay);
                scheduleDay.DeletePersonalStuff();
                
                scheduleDay.CreateAndAddDayOff(dayOff);
				_saveSchedulePartService.Save(scheduleDay, NewBusinessRuleCollection.Minimum());
                using (_messageBrokerEnablerFactory.NewMessageBrokerEnabler())
                {
                    uow.PersistAll();
                }
            }
            return new CommandResultDto { AffectedId = command.PersonId, AffectedItems = 1 };
        }

    	private IScenario getDesiredScenario(AddDayOffCommandDto command)
    	{
    		return command.ScenarioId.HasValue ? _scenarioRepository.Get(command.ScenarioId.Value) : _scenarioRepository.LoadDefaultScenario();
    	}
    }
}
