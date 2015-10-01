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
    public class AddAbsenceCommandHandlerTest
    {
        private MockRepository _mock;
        private IAssembler<DateTimePeriod, DateTimePeriodDto> _dateTimePeriodMock;
        private IAbsenceRepository _absenceRepository;
        private IScheduleRepository _scheduleRepository;
        private IPersonRepository _personRepository;
        private IScenarioRepository _scenarioRepository;
        private IUnitOfWorkFactory _unitOfWorkFactory;
        private ISaveSchedulePartService _saveSchedulePartService;
        private AddAbsenceCommandHandler _target;

        private IPerson _person;
        private IAbsence _absence;
        private IScenario _scenario;

        private static DateOnly _startDate = new DateOnly(2012, 1, 1);
        private DateOnlyPeriod _dateOnlyPeriod = new DateOnlyPeriod(_startDate, _startDate.AddDays(1));
        private DateTimePeriod _period;

        private readonly DateTimePeriodDto _periodDto = new DateTimePeriodDto
        {
            UtcStartTime = new DateTime(2012, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            UtcEndTime = new DateTime(2012, 1, 2, 0, 0, 0, DateTimeKind.Utc)
        };

    	private IUnitOfWork _unitOfWork;
    	private IScheduleDay _scheduleDay;
    	private IScheduleRange _scheduleRangeMock;
    	private IScheduleDictionary _dictionary;
    	private IBusinessRulesForPersonalAccountUpdate _businessRulesForPersonalAccountUpdate;
        private ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;
		private IScheduleTagAssembler _scheduleTagAssembler;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _dateTimePeriodMock = _mock.StrictMock<IAssembler<DateTimePeriod, DateTimePeriodDto>>();
            _absenceRepository = _mock.StrictMock<IAbsenceRepository>();
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

            _absence = AbsenceFactory.CreateAbsence("Sick");
            _absence.SetId(Guid.NewGuid());

            _scenario = ScenarioFactory.CreateScenarioAggregate();
            
            _period = _dateOnlyPeriod.ToDateTimePeriod(_person.PermissionInformation.DefaultTimeZone());

			_unitOfWork = _mock.DynamicMock<IUnitOfWork>();
			var schedulePartFactoryForDomain = new SchedulePartFactoryForDomain(_person, _scenario, _period, SkillFactory.CreateSkill("Test Skill"));
			_scheduleDay = schedulePartFactoryForDomain.CreatePart();
			_scheduleRangeMock = _mock.DynamicMultiMock<IScheduleRange>();

			_dictionary = _mock.DynamicMock<IScheduleDictionary>();
   
            _target = new AddAbsenceCommandHandler(_dateTimePeriodMock, _absenceRepository, _scheduleRepository, _personRepository, _scenarioRepository, _currentUnitOfWorkFactory, _saveSchedulePartService, _businessRulesForPersonalAccountUpdate, _scheduleTagAssembler);
        }

        [Test]
        public void AbsenceIsAddedSuccessfully()
        {
            var addAbsenceCommandDto = new AddAbsenceCommandDto
                                           {
                                               Period = _periodDto,
                                               AbsenceId = _absence.Id.GetValueOrDefault(),
                                               PersonId = _person.Id.GetValueOrDefault(),
											   Date = new DateOnlyDto { DateTime = _startDate.Date },
                                               ScheduleTagId = Guid.NewGuid()

                                           };
        	var rules = _mock.DynamicMock<INewBusinessRuleCollection>();

            var scheduleTag = new ScheduleTag {Description = "test"};

            using (_mock.Record())
            {
                Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(_unitOfWork);
                Expect.Call(_currentUnitOfWorkFactory.Current()).Return(_unitOfWorkFactory);
                Expect.Call(_personRepository.Load(_person.Id.GetValueOrDefault())).Return(_person);
                Expect.Call(_scenarioRepository.LoadDefaultScenario()).Return(_scenario);
				Expect.Call(_scheduleRepository.FindSchedulesForPersonOnlyInGivenPeriod(null, null, _dateOnlyPeriod, _scenario)).IgnoreArguments().Return(_dictionary);
                Expect.Call(_absenceRepository.Load(_absence.Id.GetValueOrDefault())).Return(_absence);
                Expect.Call(_scheduleRangeMock.ScheduledDay(_startDate)).Return(_scheduleDay);
                Expect.Call(_dictionary[_person]).Return(_scheduleRangeMock);
                Expect.Call(_dateTimePeriodMock.DtoToDomainEntity(_periodDto)).IgnoreArguments().Return(_period);
				Expect.Call(_businessRulesForPersonalAccountUpdate.FromScheduleRange(_scheduleRangeMock)).Return(rules);
                Expect.Call(_scheduleTagAssembler.DtoToDomainEntity(null)).IgnoreArguments().Return(scheduleTag);
                Expect.Call(()=>_saveSchedulePartService.Save(_scheduleDay,rules, scheduleTag));
            }
            using (_mock.Playback())
            {
                _target.Handle(addAbsenceCommandDto);
                _scheduleDay.PersonAbsenceCollection().Count.Should().Be.EqualTo(1);
            }
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void AbsenceIsAddedToAnotherScenarioSuccessfully()
		{
			var scenarioId = Guid.NewGuid();
            var scheduleTag = new ScheduleTag { Description = "test" };
			var addAbsenceCommandDto = new AddAbsenceCommandDto
			{
				Period = _periodDto,
				AbsenceId = _absence.Id.GetValueOrDefault(),
				PersonId = _person.Id.GetValueOrDefault(),
				Date = new DateOnlyDto { DateTime = _startDate.Date },
                ScheduleTagId = Guid.NewGuid(),
				ScenarioId = scenarioId
			};
			var rules = _mock.DynamicMock<INewBusinessRuleCollection>();

			using (_mock.Record())
			{
				Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(_unitOfWork);
				Expect.Call(_currentUnitOfWorkFactory.Current()).Return(_unitOfWorkFactory);
				Expect.Call(_personRepository.Load(_person.Id.GetValueOrDefault())).Return(_person);
				Expect.Call(_scenarioRepository.Get(scenarioId)).Return(_scenario);
				Expect.Call(_scheduleRepository.FindSchedulesForPersonOnlyInGivenPeriod(null, null, _dateOnlyPeriod, _scenario)).IgnoreArguments().Return(_dictionary);
				Expect.Call(_absenceRepository.Load(_absence.Id.GetValueOrDefault())).Return(_absence);
				Expect.Call(_scheduleRangeMock.ScheduledDay(_startDate)).Return(_scheduleDay);
				Expect.Call(_dictionary[_person]).Return(_scheduleRangeMock);
				Expect.Call(_dateTimePeriodMock.DtoToDomainEntity(_periodDto)).IgnoreArguments().Return(_period);
				Expect.Call(_businessRulesForPersonalAccountUpdate.FromScheduleRange(_scheduleRangeMock)).Return(rules);
				Expect.Call(_scheduleTagAssembler.DtoToDomainEntity(null)).IgnoreArguments().Return(scheduleTag);
				Expect.Call(() => _saveSchedulePartService.Save(_scheduleDay,rules, scheduleTag));
			}
			using (_mock.Playback())
			{
				_target.Handle(addAbsenceCommandDto);
				_scheduleDay.PersonAbsenceCollection().Count.Should().Be.EqualTo(1);
			}
		}
    }
}
