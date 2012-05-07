using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
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
    public class AddDayOffCommandHandlerTest
    {
        private IDayOffRepository _dayOffRepository;
        private IScheduleRepository _scheduleRepository;
        private IPersonRepository _personRepository;
        private IScenarioRepository _scenarioRepository;
        private IUnitOfWorkFactory _unitOfWorkFactory;
        private ISaveSchedulePartService _saveSchedulePartService;
        private IMessageBrokerEnablerFactory _messageBrokerEnablerFactory;
        private MockRepository _mock;
        private AddDayOffCommandHandler _target;


        [SetUp]
        public  void Setup()
        {
            _mock = new MockRepository();
            _dayOffRepository = _mock.StrictMock<IDayOffRepository>();
            _scheduleRepository = _mock.StrictMock<IScheduleRepository>();
            _personRepository = _mock.StrictMock<IPersonRepository>();
            _scenarioRepository = _mock.StrictMock<IScenarioRepository>();
            _unitOfWorkFactory = _mock.StrictMock<IUnitOfWorkFactory>();
            _saveSchedulePartService = _mock.StrictMock<ISaveSchedulePartService>();
            _messageBrokerEnablerFactory = _mock.DynamicMock<IMessageBrokerEnablerFactory>();
            _target = new AddDayOffCommandHandler(_dayOffRepository,_scheduleRepository,_personRepository,_scenarioRepository,_unitOfWorkFactory,_saveSchedulePartService,_messageBrokerEnablerFactory);
        }

        [Test]
        public void ShouldAddDayOffInTheDictionarySuccessfully()
        {
            var unitOfWork = _mock.DynamicMock<IUnitOfWork>();
            IPerson person = PersonFactory.CreatePerson();
            person.SetId(Guid.NewGuid());
            
            var scenario = ScenarioFactory.CreateScenarioAggregate();
            var timeZone = person.PermissionInformation.DefaultTimeZone();
            var startDate = new DateOnly(2012, 1, 1);
            var dateOnydto = new DateOnlyDto(startDate);
            var dateOnlyPeriod = new DateOnlyPeriod(startDate, startDate.AddDays(1));
            var period = dateOnlyPeriod.ToDateTimePeriod(person.PermissionInformation.DefaultTimeZone());
            var schedulePartFactory = new SchedulePartFactoryForDomain(person, scenario, period, SkillFactory.CreateSkill("Test Skill"));
            var scheduleDay = schedulePartFactory.CreatePartWithMainShift();
            var scheduleRangeMock = _mock.DynamicMock<IScheduleRange>();

            var dictionary = new ScheduleDictionaryForTest(scenario, new ScheduleDateTimePeriod(period),
                                                           new Dictionary<IPerson, IScheduleRange> { { person, scheduleRangeMock } });

            var dayOff = DayOffFactory.CreateDayOff();
            dayOff.SetId(Guid.NewGuid());

            var addAbsenceCommandDto = new AddDayOffCommandDto
                                           {
                                               PersonId = person.Id.GetValueOrDefault(),
                                               Date = dateOnydto,
                                               DayOffInfoId = dayOff.Id.GetValueOrDefault()
                                           };

            using (_mock.Record())
            {
                Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
                Expect.Call(_personRepository.Load(person.Id.GetValueOrDefault())).Return(person);
                Expect.Call(_scenarioRepository.LoadDefaultScenario()).Return(scenario);
                Expect.Call(_scheduleRepository.FindSchedulesOnlyInGivenPeriod(
                    new PersonProvider(new[] { person }), new ScheduleDictionaryLoadOptions(false, false),
                    new DateOnlyPeriod(startDate, startDate.AddDays(1)).ToDateTimePeriod(timeZone), scenario)).
                    IgnoreArguments().Return(dictionary);
                Expect.Call(_dayOffRepository.Load(dayOff.Id.GetValueOrDefault())).Return(dayOff);
                Expect.Call(scheduleRangeMock.ScheduledDay(startDate)).Return(scheduleDay);
                Expect.Call(() => _saveSchedulePartService.Save(unitOfWork, scheduleDay));
            }
            using (_mock.Playback())
            {
                _target.Handle(addAbsenceCommandDto);
                scheduleDay.PersonDayOffCollection().Count.Should().Be.EqualTo(1);
            }

        }
    }
}
