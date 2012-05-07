using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
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
    public class NewMainShiftCommandHandlerTest
    {
        private MockRepository _mock;
        private IUnitOfWorkFactory _unitOfWorkFactory;
        private IShiftCategoryRepository _shiftCategoryRepository;
        private IActivityLayerAssembler<IMainShiftActivityLayer> _mainActivityLayerAssembler;
        private IScheduleRepository _scheduleRepository;
        private IScenarioRepository _scenarioRepository;
        private IPersonRepository _personRepository;
        private ISaveSchedulePartService _saveSchedulePartService;
        private IMessageBrokerEnablerFactory _messageBrokerEnablerFactory;
        private NewMainShiftCommandHandler _target;


        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _unitOfWorkFactory = _mock.StrictMock<IUnitOfWorkFactory>();
            _shiftCategoryRepository = _mock.StrictMock<IShiftCategoryRepository>();
            _mainActivityLayerAssembler = _mock.StrictMock<IActivityLayerAssembler<IMainShiftActivityLayer>>();
            _scheduleRepository = _mock.StrictMock<IScheduleRepository>();
            _scenarioRepository = _mock.StrictMock<IScenarioRepository>();
            _personRepository = _mock.StrictMock<IPersonRepository>();
            _saveSchedulePartService = _mock.StrictMock<ISaveSchedulePartService>();
            _messageBrokerEnablerFactory = _mock.DynamicMock<IMessageBrokerEnablerFactory>();
            _target = new NewMainShiftCommandHandler(_unitOfWorkFactory,_shiftCategoryRepository,_mainActivityLayerAssembler,_scheduleRepository,_scenarioRepository,_personRepository,_saveSchedulePartService,_messageBrokerEnablerFactory);
        }

        [Test]
        public void NewMainShiftShouldBeAddedSuccessfully()
        {
            var unitOfWork = _mock.DynamicMock<IUnitOfWork>();
            var startDate = new DateTime(2012, 1, 1,0,0,0,DateTimeKind.Utc);
            var dateOnlydto = new DateOnlyDto(new DateOnly(startDate));
            var person = PersonFactory.CreatePerson("test");
            person.SetId(Guid.NewGuid());
            var timeZone = person.PermissionInformation.DefaultTimeZone();
            var scenario = ScenarioFactory.CreateScenarioAggregate();
            scenario.SetId(Guid.NewGuid());

            var shiftCategory = ShiftCategoryFactory.CreateShiftCategory("test Shift Category");
            shiftCategory.SetId(Guid.NewGuid());
         
            var newMainShiftCommandDto = new NewMainShiftCommandDto
                                             {
                                                 Date = dateOnlydto,
                                                 PersonId = person.Id.GetValueOrDefault(),
                                                 ShiftCategoryId = shiftCategory.Id.GetValueOrDefault()
                                             };

            var period = new DateTimePeriod(startDate, startDate.AddDays(1));
            var scheduleRange = new SchedulePartFactoryForDomain(person, scenario, period, SkillFactory.CreateSkill("Test Skill"));
            var schedulePart = scheduleRange.CreatePart();
            var scheduleRangeMock = _mock.DynamicMock<IScheduleRange>();
            var dictionary = new ScheduleDictionaryForTest(scenario, new ScheduleDateTimePeriod(period),
                                                           new Dictionary<IPerson, IScheduleRange> { { person, scheduleRangeMock } });
           

            using(_mock.Record())
            {
                Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
                Expect.Call(_personRepository.Load(newMainShiftCommandDto.PersonId)).Return(person);
                Expect.Call(_scenarioRepository.LoadDefaultScenario()).Return(scenario);
                Expect.Call(_mainActivityLayerAssembler.DtosToDomainEntities(new Collection<ActivityLayerDto>())).
                    IgnoreArguments().Return(new Collection<IMainShiftActivityLayer>());
                Expect.Call(_scheduleRepository.FindSchedulesOnlyInGivenPeriod(
                    new PersonProvider(new[] { person }), new ScheduleDictionaryLoadOptions(false, false),
                    new DateOnlyPeriod(new DateOnly(startDate), new DateOnly(startDate.AddDays(1))).ToDateTimePeriod(timeZone), scenario)).
                    IgnoreArguments().Return(dictionary);
                Expect.Call(scheduleRangeMock.ScheduledDay(new DateOnly(startDate))).Return(schedulePart);
                Expect.Call(_shiftCategoryRepository.Load(newMainShiftCommandDto.ShiftCategoryId)).Return(shiftCategory);
                Expect.Call(()=>_saveSchedulePartService.Save(unitOfWork, schedulePart));
            }
            using(_mock.Playback())
            {
                _target.Handle(newMainShiftCommandDto);
            }
        }

    }
}
