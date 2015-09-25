using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Ccc.Sdk.Logic;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Ccc.Sdk.Logic.CommandHandler;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.LogicTest.CommandHandler
{
    [TestFixture]
    public class AddActivityCommandHandlerTest
    {

        private MockRepository _mock;
        private IAssembler<DateTimePeriod, DateTimePeriodDto> _dateTimePeriodMock;
        private IActivityRepository _activityRepository;
        private IScheduleRepository _scheduleRepository;
        private IPersonRepository _personRepository;
        private IScenarioRepository _scenarioRepository;
        private IUnitOfWorkFactory _unitOfWorkFactory;
        private ISaveSchedulePartService _saveSchedulePartService;
        private AddActivityCommandHandler _target;
        private IPerson _person;
        private IActivity _activity;
        private IScenario _scenario;
        private static DateOnly _startDate = new DateOnly(2012, 1, 1);
		private readonly DateOnlyDto _dateOnlyDto = new DateOnlyDto { DateTime = _startDate.Date };
        private readonly DateTimePeriodDto _periodDto = new DateTimePeriodDto
        {
            UtcStartTime = new DateTime(2012, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            UtcEndTime = new DateTime(2012, 1, 2, 0, 0, 0, DateTimeKind.Utc)
        };

        private static DateOnlyPeriod _dateOnlyPeriod = new DateOnlyPeriod(_startDate, _startDate.AddDays(1));
        private DateTimePeriod _period;
        private SchedulePartFactoryForDomain _schedulePartFactory;
        private AddActivityCommandDto _addAbsenceCommandDto;
    	private IBusinessRulesForPersonalAccountUpdate _businessRulesForPersonalAccountUpdate;
        private ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;
		private IScheduleTagAssembler _scheduleTagAssembler;

        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _dateTimePeriodMock = _mock.StrictMock<IAssembler<DateTimePeriod, DateTimePeriodDto>>();
            _activityRepository = _mock.StrictMock<IActivityRepository>();
            _scheduleRepository = _mock.StrictMock<IScheduleRepository>();
            _personRepository = _mock.StrictMock<IPersonRepository>();
            _scenarioRepository = _mock.StrictMock<IScenarioRepository>();
            _unitOfWorkFactory = _mock.StrictMock<IUnitOfWorkFactory>();
            _currentUnitOfWorkFactory = _mock.DynamicMock<ICurrentUnitOfWorkFactory>();
            _saveSchedulePartService = _mock.StrictMock<ISaveSchedulePartService>();
    		_businessRulesForPersonalAccountUpdate = _mock.DynamicMock<IBusinessRulesForPersonalAccountUpdate>();
			_scheduleTagAssembler = _mock.DynamicMock<IScheduleTagAssembler>();

            _person = PersonFactory.CreatePerson();
            _person.SetId(Guid.NewGuid());

            _activity = ActivityFactory.CreateActivity("Test Activity");
            _activity.SetId(Guid.NewGuid());

            _scenario = ScenarioFactory.CreateScenarioAggregate();
            _period = _dateOnlyPeriod.ToDateTimePeriod(_person.PermissionInformation.DefaultTimeZone());

            _schedulePartFactory = new SchedulePartFactoryForDomain(_person, _scenario, _period, SkillFactory.CreateSkill("Test Skill"));
			_target = new AddActivityCommandHandler(_dateTimePeriodMock, _activityRepository, _scheduleRepository, _personRepository, _scenarioRepository, _currentUnitOfWorkFactory, _saveSchedulePartService, _businessRulesForPersonalAccountUpdate, _scheduleTagAssembler);

            _addAbsenceCommandDto = new AddActivityCommandDto
            {
                Period = _periodDto,
                ActivityId = _activity.Id.GetValueOrDefault(),
                PersonId = _person.Id.GetValueOrDefault(),
                Date = _dateOnlyDto,
                ScheduleTagId = Guid.NewGuid()
            };
        }

        [Test]
        public void ShouldAddActivityInTheDictionarySuccessfully()
        {
            var unitOfWork = _mock.DynamicMock<IUnitOfWork>();
            var schedulePart = _schedulePartFactory.CreatePartWithMainShift();
            var scheduleRangeMock = _mock.DynamicMock<IScheduleRange>();
            var dictionary = _mock.DynamicMock<IScheduleDictionary>();
        	var rules = _mock.DynamicMock<INewBusinessRuleCollection>();
            var scheduleTag = new ScheduleTag() { Description = "test" };
            
            using (_mock.Record())
            {
                Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
                Expect.Call(_currentUnitOfWorkFactory.Current()).Return(_unitOfWorkFactory);
                Expect.Call(_personRepository.Load(_person.Id.GetValueOrDefault())).Return(_person);
                Expect.Call(_scenarioRepository.LoadDefaultScenario()).Return(_scenario);
				Expect.Call(_scheduleRepository.FindSchedulesForPersonOnlyInGivenPeriod(null, null, _dateOnlyPeriod, _scenario)).IgnoreArguments().Return(dictionary);
                Expect.Call(_activityRepository.Load(_activity.Id.GetValueOrDefault())).Return(_activity);
                Expect.Call(scheduleRangeMock.ScheduledDay(_startDate)).Return(schedulePart);
                Expect.Call(dictionary[_person]).Return(scheduleRangeMock);
                Expect.Call(_dateTimePeriodMock.DtoToDomainEntity(_periodDto)).Return(_period);
            	Expect.Call(_businessRulesForPersonalAccountUpdate.FromScheduleRange(scheduleRangeMock)).Return(rules);
				Expect.Call(_scheduleTagAssembler.DtoToDomainEntity(null)).IgnoreArguments().Return(scheduleTag);
                Expect.Call(() => _saveSchedulePartService.Save(schedulePart,rules, scheduleTag));
            }
            using (_mock.Playback())
            {
                _target.Handle(_addAbsenceCommandDto);
                schedulePart.PersonAssignment().Should().Not.Be.Null();
            }
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "ForGiven"), Test]
		public void ShouldAddActivityInTheDictionaryForGivenScenarioSuccessfully()
		{
			var scenarioId = Guid.NewGuid();
			var unitOfWork = _mock.DynamicMock<IUnitOfWork>();
			var schedulePart = _schedulePartFactory.CreatePartWithMainShift();
			var scheduleRangeMock = _mock.DynamicMock<IScheduleRange>();
			var dictionary = _mock.DynamicMock<IScheduleDictionary>();
			var rules = _mock.DynamicMock<INewBusinessRuleCollection>();
            var scheduleTag = new ScheduleTag() {Description = "test"};

			using (_mock.Record())
			{
				Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
				Expect.Call(_currentUnitOfWorkFactory.Current()).Return(_unitOfWorkFactory);
				Expect.Call(_personRepository.Load(_person.Id.GetValueOrDefault())).Return(_person);
				Expect.Call(_scenarioRepository.Get(scenarioId)).Return(_scenario);
				Expect.Call(_scheduleRepository.FindSchedulesForPersonOnlyInGivenPeriod(null, null, _dateOnlyPeriod, _scenario)).IgnoreArguments().Return(dictionary);
				Expect.Call(_activityRepository.Load(_activity.Id.GetValueOrDefault())).Return(_activity);
				Expect.Call(scheduleRangeMock.ScheduledDay(_startDate)).Return(schedulePart);
				Expect.Call(dictionary[_person]).Return(scheduleRangeMock);
				Expect.Call(_dateTimePeriodMock.DtoToDomainEntity(_periodDto)).Return(_period);
				Expect.Call(_businessRulesForPersonalAccountUpdate.FromScheduleRange(scheduleRangeMock)).Return(rules);
				Expect.Call(_scheduleTagAssembler.DtoToDomainEntity(null)).IgnoreArguments().Return(scheduleTag);
				Expect.Call(() => _saveSchedulePartService.Save(schedulePart,rules, scheduleTag ));
			}
			using (_mock.Playback())
			{
				_addAbsenceCommandDto.ScenarioId = scenarioId;
				_target.Handle(_addAbsenceCommandDto);
				schedulePart.PersonAssignment().Should().Not.Be.Null();
			}
		}
    }
}
