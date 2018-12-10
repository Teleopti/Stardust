using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Ccc.Sdk.Logic.CommandHandler;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.Sdk.LogicTest.CommandHandler
{
    [TestFixture]
    public class AddOvertimeCommandHandlerTest
    {
        private MockRepository _mock;
        private IAssembler<DateTimePeriod, DateTimePeriodDto> _dateTimePeriodMock;
        private IActivityRepository _activityRepository;
        private IScheduleStorage _scheduleStorage;
        private IPersonRepository _personRepository;
        private IScenarioRepository _scenarioRepository;
        private IUnitOfWorkFactory _unitOfWorkFactory;
        private AddOvertimeCommandHandler _target;
        private IMultiplicatorDefinitionSetRepository _multiplicatorDefinitionSetRepository;
        private IPerson _person;
        private IActivity _activity;
        private IScenario _scenario;
        private static DateOnly _startDate = new DateOnly(2012, 1, 1);
		private readonly DateOnlyDto _dateOnyldto = new DateOnlyDto { DateTime = _startDate.Date };
        private readonly DateTimePeriodDto _periodDto = new DateTimePeriodDto
        {
            UtcStartTime = new DateTime(2012, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            UtcEndTime = new DateTime(2012, 1, 2, 0, 0, 0, DateTimeKind.Utc)
        };

        private static DateOnlyPeriod _dateOnlyPeriod = new DateOnlyPeriod(_startDate, _startDate.AddDays(1));
        private DateTimePeriod _period;
        private MultiplicatorDefinitionSet _multiplicatorDefinitionSet;
        private AddOvertimeCommandDto _addOvertimeCommandDto;
    	private IBusinessRulesForPersonalAccountUpdate _businessRulesForPersonalAccountUpdate;
        private ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;
		private IScheduleTagAssembler _scheduleTagAssembler;
		private IScheduleSaveHandler _scheduleSaveHandler;

	    [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _dateTimePeriodMock = _mock.StrictMock<IAssembler<DateTimePeriod, DateTimePeriodDto>>();
            _multiplicatorDefinitionSetRepository = _mock.StrictMock<IMultiplicatorDefinitionSetRepository>();
            _activityRepository = _mock.StrictMock<IActivityRepository>();
            _scheduleStorage = _mock.StrictMock<IScheduleStorage>();
            _personRepository = _mock.StrictMock<IPersonRepository>();
            _scenarioRepository = _mock.StrictMock<IScenarioRepository>();
            _unitOfWorkFactory = _mock.StrictMock<IUnitOfWorkFactory>();
            _currentUnitOfWorkFactory = _mock.DynamicMock<ICurrentUnitOfWorkFactory>();
    		_businessRulesForPersonalAccountUpdate = _mock.DynamicMock<IBusinessRulesForPersonalAccountUpdate>();
			_scheduleTagAssembler = _mock.DynamicMock<IScheduleTagAssembler>();
			_scheduleSaveHandler = _mock.StrictMock<IScheduleSaveHandler>();

            _person = PersonFactory.CreatePerson();
            _person.SetId(Guid.NewGuid());

            _activity = ActivityFactory.CreateActivity("Test Activity");
            _activity.SetId(Guid.NewGuid());

            _scenario = ScenarioFactory.CreateScenarioAggregate();
            _period = _dateOnlyPeriod.ToDateTimePeriod(_person.PermissionInformation.DefaultTimeZone());
            _multiplicatorDefinitionSet = new MultiplicatorDefinitionSet("test", MultiplicatorType.Overtime);
				_target = new AddOvertimeCommandHandler(_dateTimePeriodMock, _multiplicatorDefinitionSetRepository, _activityRepository, _scheduleStorage, _personRepository, _scenarioRepository, _currentUnitOfWorkFactory, _businessRulesForPersonalAccountUpdate, _scheduleTagAssembler, _scheduleSaveHandler);

            _addOvertimeCommandDto = new AddOvertimeCommandDto
            {
                ActivityId = _activity.Id.GetValueOrDefault(),
                Date = _dateOnyldto,
                Period = _periodDto,
                PersonId = _person.Id.GetValueOrDefault(),
                OvertimeDefinitionSetId = Guid.NewGuid(),
                ScheduleTagId = Guid.NewGuid()
            };
        }

        [Test]
        public void ShouldAddOvertimeInTheDictionarySuccessfully()
        {
            var unitOfWork = _mock.DynamicMock<IUnitOfWork>();
            var scheduleRangeMock = _mock.DynamicMock<IScheduleRange>();
            var scheduleDay = _mock.StrictMock<IScheduleDay>();
            var dictionary = _mock.DynamicMock<IScheduleDictionary>();
        	var rules = _mock.DynamicMock<INewBusinessRuleCollection>();
            var scheduleTag = new ScheduleTag { Description = "test" };
            using (_mock.Record())
            {
                Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
                Expect.Call(_currentUnitOfWorkFactory.Current()).Return(_unitOfWorkFactory);
                Expect.Call(_personRepository.Load(_person.Id.GetValueOrDefault())).Return(_person);
                Expect.Call(_scenarioRepository.LoadDefaultScenario()).Return(_scenario);
				Expect.Call(_scheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(null, null, _dateOnlyPeriod, _scenario)).IgnoreArguments().Return(dictionary);
                Expect.Call(dictionary[_person]).Return(scheduleRangeMock);
                Expect.Call(_activityRepository.Load(_activity.Id.GetValueOrDefault())).Return(_activity);
                Expect.Call(_multiplicatorDefinitionSetRepository.Load(_addOvertimeCommandDto.OvertimeDefinitionSetId)).Return(_multiplicatorDefinitionSet);
                Expect.Call(()=> scheduleDay.CreateAndAddOvertime(null, new DateTimePeriod(), null)).IgnoreArguments();
                Expect.Call(scheduleRangeMock.ScheduledDay(_startDate)).Return(scheduleDay);
                Expect.Call(_dateTimePeriodMock.DtoToDomainEntity(_periodDto)).Return(_period);
            	Expect.Call(_businessRulesForPersonalAccountUpdate.FromScheduleRange(scheduleRangeMock)).Return(rules);
				Expect.Call(_scheduleTagAssembler.DtoToDomainEntity(null)).IgnoreArguments().Return(scheduleTag);
	            Expect.Call(() => _scheduleSaveHandler.ProcessSave(scheduleDay, rules, scheduleTag));
            }
            using (_mock.Playback())
            {
                _target.Handle(_addOvertimeCommandDto);
            }
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "ForGiven"), Test]
		public void ShouldAddOvertimeInTheDictionaryForGivenScenarioSuccessfully()
		{
			var scenarioId = Guid.NewGuid();
			var unitOfWork = _mock.DynamicMock<IUnitOfWork>();
			var scheduleRangeMock = _mock.DynamicMock<IScheduleRange>();
			var scheduleDay = _mock.StrictMock<IScheduleDay>();
			var dictionary = _mock.DynamicMock<IScheduleDictionary>();
			var rules = _mock.DynamicMock<INewBusinessRuleCollection>();
            var scheduleTag = new ScheduleTag { Description = "test" };
			using (_mock.Record())
			{
				Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
				Expect.Call(_currentUnitOfWorkFactory.Current()).Return(_unitOfWorkFactory);
				Expect.Call(_personRepository.Load(_person.Id.GetValueOrDefault())).Return(_person);
				Expect.Call(_scenarioRepository.Get(scenarioId)).Return(_scenario);
				Expect.Call(_scheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(null, null, _dateOnlyPeriod, _scenario)).IgnoreArguments().Return(dictionary);
				Expect.Call(dictionary[_person]).Return(scheduleRangeMock);
				Expect.Call(_activityRepository.Load(_activity.Id.GetValueOrDefault())).Return(_activity);
				Expect.Call(_multiplicatorDefinitionSetRepository.Load(_addOvertimeCommandDto.OvertimeDefinitionSetId)).Return(_multiplicatorDefinitionSet);
				Expect.Call(() => scheduleDay.CreateAndAddOvertime(null, new DateTimePeriod(), null)).IgnoreArguments();
				Expect.Call(scheduleRangeMock.ScheduledDay(_startDate)).Return(scheduleDay);
				Expect.Call(_dateTimePeriodMock.DtoToDomainEntity(_periodDto)).Return(_period);
				Expect.Call(_businessRulesForPersonalAccountUpdate.FromScheduleRange(scheduleRangeMock)).Return(rules);
				Expect.Call(_scheduleTagAssembler.DtoToDomainEntity(null)).IgnoreArguments().Return(scheduleTag);
				Expect.Call(() => _scheduleSaveHandler.ProcessSave(scheduleDay, rules, scheduleTag));
			}
			using (_mock.Playback())
			{
				_addOvertimeCommandDto.ScenarioId = scenarioId;
				_target.Handle(_addOvertimeCommandDto);
			}
		}
    }
}
