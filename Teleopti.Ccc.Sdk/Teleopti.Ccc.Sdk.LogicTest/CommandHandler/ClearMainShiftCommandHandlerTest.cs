using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Ccc.Sdk.Logic;
using Teleopti.Ccc.Sdk.Logic.CommandHandler;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.LogicTest.CommandHandler
{
    [TestFixture]
    public class ClearMainShiftCommandHandlerTest
    {
        private MockRepository _mock;
        private IScheduleRepository _scheduleRepository;
        private IPersonRepository _personRepository;
        private IScenarioRepository _scenarioRepository;
        private IUnitOfWorkFactory _unitOfWorkFactory;
        private ISaveSchedulePartService _saveSchedulePartService;
        private IMessageBrokerEnablerFactory _messageBrokerEnablerFactory;
        private ClearMainShiftCommandHandler _target ;
        private static DateTime _startDate = new DateTime(2012, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private readonly DateOnlyDto _dateOnlydto = new DateOnlyDto(new DateOnly(_startDate));
        private IPerson _person;
        private IScenario _scenario;
        private ClearMainShiftCommandDto _clearMainShiftDto;
        private readonly DateTimePeriod _period = new DateTimePeriod(_startDate, _startDate.AddDays(1));
        private SchedulePartFactoryForDomain _scheduleRange;

        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _scheduleRepository = _mock.StrictMock<IScheduleRepository>();
            _personRepository = _mock.StrictMock<IPersonRepository>();
            _scenarioRepository = _mock.StrictMock<IScenarioRepository>();
            _unitOfWorkFactory = _mock.StrictMock<IUnitOfWorkFactory>();
            _saveSchedulePartService = _mock.StrictMock<ISaveSchedulePartService>();
            _messageBrokerEnablerFactory = _mock.DynamicMock<IMessageBrokerEnablerFactory>();
            _target = new ClearMainShiftCommandHandler(_scheduleRepository,_personRepository,_scenarioRepository,_unitOfWorkFactory,_saveSchedulePartService,_messageBrokerEnablerFactory);

            _person = PersonFactory.CreatePerson("test");
            _person.SetId(Guid.NewGuid());

            _scenario = ScenarioFactory.CreateScenarioAggregate();
            _scenario.SetId(Guid.NewGuid());

            _clearMainShiftDto = new ClearMainShiftCommandDto { Date = _dateOnlydto, PersonId = _person.Id.GetValueOrDefault() };
            _scheduleRange = new SchedulePartFactoryForDomain(_person, _scenario, _period, SkillFactory.CreateSkill("Test Skill"));
        }

        [Test]
        public void ClearMainShiftFromTheDictionarySuccessfully()
        {
            var unitOfWork = _mock.DynamicMock<IUnitOfWork>();
            var schedulePart = _scheduleRange.CreatePart();
            var scheduleRangeMock = _mock.DynamicMock<IScheduleRange>();
            var dictionary = _mock.DynamicMock<IScheduleDictionary>();

            using (_mock.Record())
            {
                Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
                Expect.Call(_personRepository.Load(_clearMainShiftDto.PersonId)).Return(_person);
                Expect.Call(_scenarioRepository.LoadDefaultScenario()).Return(_scenario);
                Expect.Call(_scheduleRepository.FindSchedulesOnlyInGivenPeriod(null, null, _period, _scenario)).
                    IgnoreArguments().Return(dictionary);
                Expect.Call(dictionary[_person]).Return(scheduleRangeMock);
                Expect.Call(scheduleRangeMock.ScheduledDay(new DateOnly(_startDate))).Return(schedulePart);
                Expect.Call(() => schedulePart.DeleteMainShift(schedulePart));
                Expect.Call(() => _saveSchedulePartService.Save(schedulePart,null)).IgnoreArguments();
            }
            using(_mock.Playback())
            {
                _target.Handle(_clearMainShiftDto);
            }
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "ForGiven"), Test]
		public void ClearMainShiftFromTheDictionaryForGivenScenarioSuccessfully()
		{
			var scenarioId = Guid.NewGuid();
			var unitOfWork = _mock.DynamicMock<IUnitOfWork>();
			var schedulePart = _scheduleRange.CreatePart();
			var scheduleRangeMock = _mock.DynamicMock<IScheduleRange>();
			var dictionary = _mock.DynamicMock<IScheduleDictionary>();

			using (_mock.Record())
			{
				Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
				Expect.Call(_personRepository.Load(_clearMainShiftDto.PersonId)).Return(_person);
				Expect.Call(_scenarioRepository.Get(scenarioId)).Return(_scenario);
				Expect.Call(_scheduleRepository.FindSchedulesOnlyInGivenPeriod(null, null, _period, _scenario)).
					IgnoreArguments().Return(dictionary);
				Expect.Call(dictionary[_person]).Return(scheduleRangeMock);
				Expect.Call(scheduleRangeMock.ScheduledDay(new DateOnly(_startDate))).Return(schedulePart);
				Expect.Call(() => schedulePart.DeleteMainShift(schedulePart));
				Expect.Call(() => _saveSchedulePartService.Save(schedulePart,null)).IgnoreArguments();
			}
			using (_mock.Playback())
			{
				_clearMainShiftDto.ScenarioId = scenarioId;
				_target.Handle(_clearMainShiftDto);
			}
		}
    }
}
