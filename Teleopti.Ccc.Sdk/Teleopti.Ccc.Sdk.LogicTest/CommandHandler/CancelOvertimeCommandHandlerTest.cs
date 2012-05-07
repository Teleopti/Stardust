using System;
using System.Collections.Generic;
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
    public class CancelOvertimeCommandHandlerTest
    {
        private MockRepository _mock;
        private IAssembler<DateTimePeriod, DateTimePeriodDto> _dateTimePeriodAssembler;
        private IScheduleRepository _scheduleRepository;
        private IPersonRepository _personRepository;
        private IScenarioRepository _scenarioRepository;
        private IUnitOfWorkFactory _unitOfWorkFactory;
        private ISaveSchedulePartService _saveSchedulePartService;
        private IMessageBrokerEnablerFactory _messageBrokerEnablerFactory;
        private CancelOvertimeCommandHandler _target;

        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _dateTimePeriodAssembler = _mock.StrictMock<IAssembler<DateTimePeriod, DateTimePeriodDto>>();
            _scheduleRepository = _mock.StrictMock<IScheduleRepository>();
            _personRepository = _mock.StrictMock<IPersonRepository>();
            _scenarioRepository = _mock.StrictMock<IScenarioRepository>();
            _unitOfWorkFactory = _mock.StrictMock<IUnitOfWorkFactory>();
            _saveSchedulePartService = _mock.DynamicMock<ISaveSchedulePartService>();
            _messageBrokerEnablerFactory = _mock.DynamicMock<IMessageBrokerEnablerFactory>();
            _target = new CancelOvertimeCommandHandler(_dateTimePeriodAssembler,_scheduleRepository,_personRepository,_scenarioRepository,_unitOfWorkFactory,_saveSchedulePartService,_messageBrokerEnablerFactory);
        }

        [Test]
        public void ShouldCancelOvertimeSuccessfully()
        {
            var unitOfWork = _mock.DynamicMock<IUnitOfWork>();
            var person = PersonFactory.CreatePerson();
            person.SetId(Guid.NewGuid());

            var activity = ActivityFactory.CreateActivity("Test Activity");
            activity.SetId(Guid.NewGuid());

            var scenario = ScenarioFactory.CreateScenarioAggregate();
            var timeZone = person.PermissionInformation.DefaultTimeZone();
            var startDate = new DateTime(2012, 1, 1,0,0,0,DateTimeKind.Utc);
            var dateOnlydto = new DateOnlyDto(new DateOnly(startDate));
            var periodDto = new DateTimePeriodDto()
            {
                UtcStartTime = startDate,
                UtcEndTime = startDate.AddDays(1)
            };

            var period = new DateTimePeriod(startDate, startDate.AddDays(1));
            var schedulePartFactoryForDomain = new SchedulePartFactoryForDomain(person, scenario, period, SkillFactory.CreateSkill("Test Skill"));
            var scheduleDay = schedulePartFactoryForDomain.CreatePart();
            var scheduleRangeMock = _mock.DynamicMock<IScheduleRange>();
            var dictionary = new ScheduleDictionaryForTest(scenario, new ScheduleDateTimePeriod(period),
                                                           new Dictionary<IPerson, IScheduleRange> { { person, scheduleRangeMock } });

            var cancelOvertimeCommandDto = new CancelOvertimeCommandDto
                                               {
                                                   Date = dateOnlydto,
                                                   Period = periodDto,
                                                   PersonId = person.Id.GetValueOrDefault()
                                               };

            using (_mock.Record())
            {
                Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
                Expect.Call(_personRepository.Load(cancelOvertimeCommandDto.PersonId)).Return(person);
                Expect.Call(_scenarioRepository.LoadDefaultScenario()).Return(scenario);
                Expect.Call(_dateTimePeriodAssembler.DtoToDomainEntity(cancelOvertimeCommandDto.Period)).Return(period);
                Expect.Call(_scheduleRepository.FindSchedulesOnlyInGivenPeriod(
                    new PersonProvider(new[] { person }), new ScheduleDictionaryLoadOptions(false, false),
                    new DateOnlyPeriod(new DateOnly(startDate),new DateOnly(startDate.AddDays(1))).ToDateTimePeriod(timeZone), scenario)).
                    IgnoreArguments().Return(dictionary);
                Expect.Call(scheduleRangeMock.ScheduledDay(new DateOnly(startDate))).Return(scheduleDay);
                Expect.Call(() => _saveSchedulePartService.Save(unitOfWork, scheduleDay));
            }
            using (_mock.Playback())
            {
                _target.Handle(cancelOvertimeCommandDto);
                scheduleDay.PersonAssignmentCollection().Count.Should().Be.EqualTo(0);
            }
        }
    }
}
