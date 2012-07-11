using System.Collections.ObjectModel;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.Logic.CommandHandler
{
    public class AddAbsenceCommandHandler : IHandleCommand<AddAbsenceCommandDto>
    {
        private readonly IAssembler<DateTimePeriod, DateTimePeriodDto> _dateTimePeriodAssembler;
        private readonly IAbsenceRepository _absenceRepository;
        private readonly IScheduleRepository _scheduleRepository;
        private readonly IPersonRepository _personRepository;
        private readonly IScenarioRepository _scenarioRepository;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly ISaveSchedulePartService _saveSchedulePartService;
    	private readonly IMessageBrokerEnablerFactory _messageBrokerEnablerFactory;
    	private readonly IPersonAbsenceAccountRepository _personAbsenceAccountRepository;

    	public AddAbsenceCommandHandler(IAssembler<DateTimePeriod, DateTimePeriodDto> dateTimePeriodAssembler, IAbsenceRepository absenceRepository, IScheduleRepository scheduleRepository, IPersonRepository personRepository, IScenarioRepository scenarioRepository, IUnitOfWorkFactory unitOfWorkFactory, ISaveSchedulePartService saveSchedulePartService, IMessageBrokerEnablerFactory messageBrokerEnablerFactory, IPersonAbsenceAccountRepository personAbsenceAccountRepository)
        {
            _dateTimePeriodAssembler = dateTimePeriodAssembler;
            _absenceRepository = absenceRepository;
            _scheduleRepository = scheduleRepository;
            _personRepository = personRepository;
            _scenarioRepository = scenarioRepository;
            _unitOfWorkFactory = unitOfWorkFactory;
            _saveSchedulePartService = saveSchedulePartService;
        	_messageBrokerEnablerFactory = messageBrokerEnablerFactory;
    		_personAbsenceAccountRepository = personAbsenceAccountRepository;
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		public CommandResultDto Handle(AddAbsenceCommandDto command)
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

				var schedulingResultStateHolder = new SchedulingResultStateHolder();
				schedulingResultStateHolder.Schedules = scheduleDictionary;
				schedulingResultStateHolder.PersonsInOrganization = new Collection<IPerson> { person };
				schedulingResultStateHolder.AllPersonAccounts = _personAbsenceAccountRepository.FindByUsers(schedulingResultStateHolder.PersonsInOrganization);

				var rules = NewBusinessRuleCollection.MinimumAndPersonAccount(schedulingResultStateHolder);
				rules.Remove(typeof(NewPersonAccountRule)); //Stop this rule from hindering a save... This will make sure that we update personal accounts.

            	var scheduleRange = scheduleDictionary[person];
				((ScheduleRange)scheduleRange).ValidateBusinessRules(rules);

				var scheduleDay = scheduleRange.ScheduledDay(startDate);
                var absence = _absenceRepository.Load(command.AbsenceId);
                var absenceLayer = new AbsenceLayer(absence, _dateTimePeriodAssembler.DtoToDomainEntity(command.Period));
                scheduleDay.CreateAndAddAbsence(absenceLayer);
                _saveSchedulePartService.Save(scheduleDay,rules);
                using (_messageBrokerEnablerFactory.NewMessageBrokerEnabler())
                {
                    uow.PersistAll();
                }
            }
            return new CommandResultDto { AffectedId = command.PersonId, AffectedItems = 1 };
        }

    	private IScenario getDesiredScenario(AddAbsenceCommandDto command)
    	{
    		return command.ScenarioId.HasValue ? _scenarioRepository.Get(command.ScenarioId.Value) : _scenarioRepository.LoadDefaultScenario();
    	}
    }
}
