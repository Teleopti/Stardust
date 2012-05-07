﻿using System;
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
        private IMessageBrokerEnablerFactory _messageBrokerEnablerFactory;
        private AddActivityCommandHandler _target;
        
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
            _saveSchedulePartService = _mock.StrictMock<ISaveSchedulePartService>();
            _messageBrokerEnablerFactory = _mock.DynamicMock<IMessageBrokerEnablerFactory>();
            _target = new AddActivityCommandHandler(_dateTimePeriodMock,_activityRepository, _scheduleRepository, _personRepository, _scenarioRepository, _unitOfWorkFactory, _saveSchedulePartService, _messageBrokerEnablerFactory);
        }

        [Test]
        public void ShouldAddActivityInTheDictionarySuccessfully()
        {
            var unitOfWork = _mock.DynamicMock<IUnitOfWork>();
            IPerson person = PersonFactory.CreatePerson();
            person.SetId(Guid.NewGuid());
            IActivity activity = ActivityFactory.CreateActivity("Test Activity");
            activity.SetId(Guid.NewGuid());
            var scenario = ScenarioFactory.CreateScenarioAggregate();
            var timeZone = person.PermissionInformation.DefaultTimeZone();
            var startDate = new DateOnly(2012, 1, 1);
            var dateOnydto = new DateOnlyDto(startDate);
            var periodDto = new DateTimePeriodDto()
            {
                UtcStartTime = new DateTime(2012, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                UtcEndTime = new DateTime(2012, 1, 2, 0, 0, 0, DateTimeKind.Utc)
            };

            var dateOnlyPeriod = new DateOnlyPeriod(startDate, startDate.AddDays(1));
            var period = dateOnlyPeriod.ToDateTimePeriod(person.PermissionInformation.DefaultTimeZone());
            var schedulePartFactory = new SchedulePartFactoryForDomain(person, scenario, period, SkillFactory.CreateSkill("Test Skill"));
            var schedulePart = schedulePartFactory.CreatePartWithMainShift();
            var scheduleRangeMock = _mock.DynamicMock<IScheduleRange>();
            
            var dictionary = new ScheduleDictionaryForTest(scenario, new ScheduleDateTimePeriod(period),
                                                           new Dictionary<IPerson, IScheduleRange> { { person, scheduleRangeMock } });

            var addAbsenceCommandDto = new AddActivityCommandDto
                                           {
                                               Period = periodDto,
                                               ActivityId = activity.Id.GetValueOrDefault(),
                                               PersonId = person.Id.GetValueOrDefault(),
                                               Date = dateOnydto
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
                Expect.Call(_activityRepository.Load(activity.Id.GetValueOrDefault())).Return(activity);
                Expect.Call(scheduleRangeMock.ScheduledDay(startDate)).Return(schedulePart);
                Expect.Call(_dateTimePeriodMock.DtoToDomainEntity(periodDto)).Return(period);
                Expect.Call(() => _saveSchedulePartService.Save(unitOfWork, schedulePart));
            }
            using (_mock.Playback())
            {
                _target.Handle(addAbsenceCommandDto);
                schedulePart.PersonAssignmentCollection().Count.Should().Be.EqualTo(1);
            }



        }


    }
}
