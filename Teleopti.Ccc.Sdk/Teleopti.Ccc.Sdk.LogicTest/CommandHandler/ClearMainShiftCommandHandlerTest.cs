using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
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
        }

        [Test]
        public void ClearMainShiftFromTheDictionarySuccessfully()
        {
            var unitOfWork = _mock.DynamicMock<IUnitOfWork>();
            var startDate = new DateTime(2012, 1, 1,0,0,0,DateTimeKind.Utc);
            var dateOnlydto = new DateOnlyDto(new DateOnly(startDate));
            var person = PersonFactory.CreatePerson("test");
            person.SetId(Guid.NewGuid());
            var timeZone = person.PermissionInformation.DefaultTimeZone();
            var scenario = ScenarioFactory.CreateScenarioAggregate();
            scenario.SetId(Guid.NewGuid());

            var clearMainShiftDto = new ClearMainShiftCommandDto();
            clearMainShiftDto.Date = dateOnlydto;
            clearMainShiftDto.PersonId = person.Id.GetValueOrDefault();

            var period = new DateTimePeriod(startDate, startDate.AddDays(1));
            var scheduleRange = new SchedulePartFactoryForDomain(person, scenario, period, SkillFactory.CreateSkill("Test Skill"));
            var schedulePart = scheduleRange.CreatePart();
            var scheduleRangeMock = _mock.DynamicMock<IScheduleRange>();
            var dictionary = new ScheduleDictionaryForTest(scenario, new ScheduleDateTimePeriod(period),
                                                           new Dictionary<IPerson, IScheduleRange> { { person, scheduleRangeMock } });

            using (_mock.Record())
            {
                Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
                Expect.Call(_personRepository.Load(clearMainShiftDto.PersonId)).Return(person);
                Expect.Call(_scenarioRepository.LoadDefaultScenario()).Return(scenario);
                Expect.Call(_scheduleRepository.FindSchedulesOnlyInGivenPeriod(
                    new PersonProvider(new[] { person }), new ScheduleDictionaryLoadOptions(false, false),
                    new DateOnlyPeriod(new DateOnly(startDate), new DateOnly(startDate.AddDays(1))).ToDateTimePeriod(timeZone), scenario)).
                    IgnoreArguments().Return(dictionary);
                Expect.Call(scheduleRangeMock.ScheduledDay(new DateOnly(startDate))).Return(schedulePart);
                Expect.Call(() => schedulePart.DeleteMainShift(schedulePart));
                Expect.Call(() => _saveSchedulePartService.Save(unitOfWork, schedulePart));
            }
            using(_mock.Playback())
            {
                _target.Handle(clearMainShiftDto);
            }
        }
    }

    
}
