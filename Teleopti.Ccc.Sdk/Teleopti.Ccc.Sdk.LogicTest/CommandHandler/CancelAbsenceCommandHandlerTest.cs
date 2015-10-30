using System;
using System.ServiceModel;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
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
    public class CancelAbsenceCommandHandlerTest
    {
        private MockRepository _mock;
        private IAssembler<DateTimePeriod, DateTimePeriodDto> _dateTimePeriodAssembler;
        private IScheduleRepository _scheduleRepository;
        private IPersonRepository _personRepository;
        private IScenarioRepository _scenarioRepository;
        private IUnitOfWorkFactory _unitOfWorkFactory;
        private ISaveSchedulePartService _saveSchedulePartService;
        private CancelAbsenceCommandHandler _target;
        private IPerson _person;
        private IAbsence _absence;
        private IScenario _scenario;
        private static DateTime _startDate = new DateTime(2012, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private readonly DateTimePeriod _period = new DateTimePeriod(_startDate, _startDate.AddDays(1));
        private readonly DateTimePeriodDto _periodDto = new DateTimePeriodDto
        {
            UtcStartTime = _startDate,
            UtcEndTime = _startDate.AddDays(1)
        };

        private SchedulePartFactoryForDomain _schedulePartFactoryForDomain;
        private CancelAbsenceCommandDto _cancelAbsenceCommandDto;
    	private IBusinessRulesForPersonalAccountUpdate _businessRulesForPersonalAccountUpdate;
        private ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;
	    private IScheduleTagAssembler _scheduleTagAssembler;


	    [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _dateTimePeriodAssembler = _mock.StrictMock<IAssembler<DateTimePeriod, DateTimePeriodDto>>();
            _scheduleRepository = _mock.StrictMock<IScheduleRepository>();
            _personRepository = _mock.StrictMock<IPersonRepository>();
            _scenarioRepository = _mock.StrictMock<IScenarioRepository>();
            _unitOfWorkFactory = _mock.StrictMock<IUnitOfWorkFactory>();
            _currentUnitOfWorkFactory = _mock.DynamicMock<ICurrentUnitOfWorkFactory>();
            _saveSchedulePartService = _mock.StrictMock<ISaveSchedulePartService>();
    		_businessRulesForPersonalAccountUpdate = _mock.DynamicMock<IBusinessRulesForPersonalAccountUpdate>();
			_scheduleTagAssembler = _mock.DynamicMock<IScheduleTagAssembler>();

            _target = new CancelAbsenceCommandHandler(_dateTimePeriodAssembler, _scheduleTagAssembler,_scheduleRepository,_personRepository,_scenarioRepository,_currentUnitOfWorkFactory,_saveSchedulePartService, _businessRulesForPersonalAccountUpdate);

            _person = PersonFactory.CreatePerson();
            _person.SetId(Guid.NewGuid());

            _absence = AbsenceFactory.CreateAbsence("Sick");
            _absence.SetId(Guid.NewGuid());

            _scenario = ScenarioFactory.CreateScenarioAggregate();
            _schedulePartFactoryForDomain = new SchedulePartFactoryForDomain(_person, _scenario, _period, SkillFactory.CreateSkill("Test Skill"));

            _cancelAbsenceCommandDto = new CancelAbsenceCommandDto { Period = _periodDto, PersonId = _person.Id.GetValueOrDefault() };
        }

		[Test]
		public void AbsenceCancelSuccessfully()
		{
			var unitOfWork = _mock.DynamicMock<IUnitOfWork>();
			var scheduleDay = _schedulePartFactoryForDomain.CreatePart();
			scheduleDay.CreateAndAddAbsence(new AbsenceLayer(_absence, _period));
			var scheduleRangeMock = _mock.DynamicMock<IScheduleRange>();
			var dictionary = _mock.DynamicMock<IScheduleDictionary>();
			var rules = _mock.DynamicMock<INewBusinessRuleCollection>();
			using (_mock.Record())
			{
				Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
				Expect.Call(_currentUnitOfWorkFactory.Current()).Return(_unitOfWorkFactory);
				Expect.Call(_personRepository.Load(_cancelAbsenceCommandDto.PersonId)).Return(_person);
				Expect.Call(_scenarioRepository.LoadDefaultScenario()).Return(_scenario);
				Expect.Call(_dateTimePeriodAssembler.DtoToDomainEntity(_cancelAbsenceCommandDto.Period)).Return(_period);
				Expect.Call(_scheduleRepository.FindSchedulesForPersonOnlyInGivenPeriod(null, null, new DateOnlyPeriod(), _scenario)).
					IgnoreArguments().Return(dictionary);
				Expect.Call(dictionary[_person]).Return(scheduleRangeMock);
				Expect.Call(scheduleRangeMock.ScheduledDay(new DateOnly(_startDate))).Return(scheduleDay);
				Expect.Call(_businessRulesForPersonalAccountUpdate.FromScheduleRange(scheduleRangeMock)).Return(rules);
				Expect.Call(() => _saveSchedulePartService.Save(scheduleDay,rules,null));
			}
			using (_mock.Playback())
			{
				_target.Handle(_cancelAbsenceCommandDto);
				scheduleDay.PersonAbsenceCollection().Count.Should().Be.EqualTo(0);
			}
		}

		[Test]
	    public void AbsenceCancelSuccessfullyForGivenType()
	    {
			var absence2 = AbsenceFactory.CreateAbsence("Holiday");
			absence2.SetId(Guid.NewGuid());
			var unitOfWork = _mock.DynamicMock<IUnitOfWork>();
			var scheduleDay = _schedulePartFactoryForDomain.CreatePart();
			scheduleDay.CreateAndAddAbsence(new AbsenceLayer(_absence, _period));
			scheduleDay.CreateAndAddAbsence(new AbsenceLayer(_absence, _period));
			scheduleDay.CreateAndAddAbsence(new AbsenceLayer(absence2, _period));
			var scheduleRangeMock = _mock.DynamicMock<IScheduleRange>();
			var dictionary = _mock.DynamicMock<IScheduleDictionary>();
			var rules = _mock.DynamicMock<INewBusinessRuleCollection>();
			using (_mock.Record())
			{
				Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
				Expect.Call(_currentUnitOfWorkFactory.Current()).Return(_unitOfWorkFactory);
				Expect.Call(_personRepository.Load(_cancelAbsenceCommandDto.PersonId)).Return(_person);
				Expect.Call(_scenarioRepository.LoadDefaultScenario()).Return(_scenario);
				Expect.Call(_dateTimePeriodAssembler.DtoToDomainEntity(_cancelAbsenceCommandDto.Period)).Return(_period);
				Expect.Call(_scheduleRepository.FindSchedulesForPersonOnlyInGivenPeriod(null, null, new DateOnlyPeriod(), _scenario)).
					IgnoreArguments().Return(dictionary);
				Expect.Call(dictionary[_person]).Return(scheduleRangeMock);
				Expect.Call(scheduleRangeMock.ScheduledDay(new DateOnly(_startDate))).Return(scheduleDay);
				Expect.Call(_businessRulesForPersonalAccountUpdate.FromScheduleRange(scheduleRangeMock)).Return(rules);
				Expect.Call(() => _saveSchedulePartService.Save(scheduleDay, rules, null));
			}
			using (_mock.Playback())
			{
				_cancelAbsenceCommandDto.AbsenceId = _absence.Id;
				scheduleDay.PersonAbsenceCollection().Count.Should().Be.EqualTo(3);
				_target.Handle(_cancelAbsenceCommandDto);
				scheduleDay.PersonAbsenceCollection().Count.Should().Be.EqualTo(1);
			}
	    }

	    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "ForGiven"), Test]
        public void AbsenceCancelSuccessfullyForGivenScenario()
        {
        	var scenarioId = Guid.NewGuid();
            var unitOfWork = _mock.DynamicMock<IUnitOfWork>();
            var scheduleDay = _schedulePartFactoryForDomain.CreatePart();
            scheduleDay.CreateAndAddAbsence(new AbsenceLayer(_absence,_period));
            var scheduleRangeMock = _mock.DynamicMock<IScheduleRange>();
            var dictionary = _mock.DynamicMock<IScheduleDictionary>();
			var rules = _mock.DynamicMock<INewBusinessRuleCollection>();
            using(_mock.Record())
            {
                Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
                Expect.Call(_currentUnitOfWorkFactory.Current()).Return(_unitOfWorkFactory);
                Expect.Call(_personRepository.Load(_cancelAbsenceCommandDto.PersonId)).Return(_person);
                Expect.Call(_scenarioRepository.Get(scenarioId)).Return(_scenario);
                Expect.Call(_dateTimePeriodAssembler.DtoToDomainEntity(_cancelAbsenceCommandDto.Period)).Return(_period);
				Expect.Call(_scheduleRepository.FindSchedulesForPersonOnlyInGivenPeriod(null, null, new DateOnlyPeriod(), _scenario)).
                    IgnoreArguments().Return(dictionary);
                Expect.Call(dictionary[_person]).Return(scheduleRangeMock);
                Expect.Call(scheduleRangeMock.ScheduledDay(new DateOnly(_startDate))).Return(scheduleDay);
				Expect.Call(_businessRulesForPersonalAccountUpdate.FromScheduleRange(scheduleRangeMock)).Return(rules);
                Expect.Call(()=>_saveSchedulePartService.Save(scheduleDay,rules,null));
            }
            using(_mock.Playback())
            {
            	_cancelAbsenceCommandDto.ScenarioId = scenarioId;
                _target.Handle(_cancelAbsenceCommandDto);
                scheduleDay.PersonAbsenceCollection().Count.Should().Be.EqualTo(0);
            }
        }

        [Test]
        [ExpectedException(typeof(FaultException))]
        public void ShouldThrowExceptionIfScheduleDayIsNull()
        {
            var unitOfWork = _mock.DynamicMock<IUnitOfWork>();
            var scheduleRangeMock = _mock.DynamicMock<IScheduleRange>();
            var dictionary = _mock.DynamicMock<IScheduleDictionary>();

            using (_mock.Record())
            {
                Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
                Expect.Call(_currentUnitOfWorkFactory.Current()).Return(_unitOfWorkFactory);
                Expect.Call(_personRepository.Load(_cancelAbsenceCommandDto.PersonId)).Return(_person);
                Expect.Call(_scenarioRepository.LoadDefaultScenario()).Return(_scenario);
                Expect.Call(_dateTimePeriodAssembler.DtoToDomainEntity(_cancelAbsenceCommandDto.Period)).Return(_period);
				Expect.Call(_scheduleRepository.FindSchedulesForPersonOnlyInGivenPeriod(null, null, new DateOnlyPeriod(), _scenario)).
                    IgnoreArguments().Return(dictionary);
                Expect.Call(dictionary[_person]).Return(scheduleRangeMock);
                Expect.Call(scheduleRangeMock.ScheduledDay(new DateOnly(_startDate))).Return(null);
            }
            using (_mock.Playback())
            {
                _target.Handle(_cancelAbsenceCommandDto);
            }
        }
    }
}
