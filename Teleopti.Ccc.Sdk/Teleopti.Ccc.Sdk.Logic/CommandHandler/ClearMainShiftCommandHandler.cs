using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.Logic.CommandHandler
{
    public class ClearMainShiftCommandHandler : IHandleCommand<ClearMainShiftCommandDto>
    {
        private readonly IScheduleRepository _scheduleRepository;
        private readonly IPersonRepository _personRepository;
        private readonly IScenarioRepository _scenarioRepository;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly ISaveSchedulePartService _saveSchedulePartService;
    	private readonly IMessageBrokerEnablerFactory _messageBrokerEnablerFactory;

    	public ClearMainShiftCommandHandler(IScheduleRepository scheduleRepository, IPersonRepository personRepository, IScenarioRepository scenarioRepository, IUnitOfWorkFactory unitOfWorkFactory, ISaveSchedulePartService saveSchedulePartService, IMessageBrokerEnablerFactory messageBrokerEnablerFactory)
        {
            _scheduleRepository = scheduleRepository;
            _personRepository = personRepository;
            _scenarioRepository = scenarioRepository;
            _unitOfWorkFactory = unitOfWorkFactory;
            _saveSchedulePartService = saveSchedulePartService;
        	_messageBrokerEnablerFactory = messageBrokerEnablerFactory;
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public CommandResultDto Handle(ClearMainShiftCommandDto command)
        {
            using (var uow = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
            {
                var person = _personRepository.Load(command.PersonId);
                var scenario = _scenarioRepository.LoadDefaultScenario();
                var timeZone = person.PermissionInformation.DefaultTimeZone();
                var startDate = new DateOnly(command.Date.DateTime);
                var scheduleDictionary = _scheduleRepository.FindSchedulesOnlyInGivenPeriod(
                    new PersonProvider(new[] {person}), new ScheduleDictionaryLoadOptions(false, false),
                    new DateOnlyPeriod(startDate, startDate.AddDays(1)).ToDateTimePeriod(timeZone), scenario);
                var scheduleDay = scheduleDictionary[person].ScheduledDay(startDate);
                scheduleDay.DeleteMainShift(scheduleDay);
                _saveSchedulePartService.Save(uow, scheduleDay);
                using (_messageBrokerEnablerFactory.NewMessageBrokerEnabler())
                {
                    uow.PersistAll();
                }
            }
            return new CommandResultDto {AffectedId = command.PersonId, AffectedItems = 1};
        }
    }
}
