using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Ccc.Sdk.Logic.CommandHandler;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.LogicTest.CommandHandler
{
	[TestFixture]
	public class AddPersonalActivityCommandHandlerTest
	{
		private MockRepository _mock;
		private IAssembler<DateTimePeriod, DateTimePeriodDto> _dateTimePeriodMock;
		private IActivityRepository _activityRepository;
		private IScheduleStorage _scheduleStorage;
		private IPersonRepository _personRepository;
		private IScenarioRepository _scenarioRepository;
		private IUnitOfWorkFactory _unitOfWorkFactory;
		private AddPersonalActivityCommandHandler _target;
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
		private AddPersonalActivityCommandDto _addPersonalActivityCommand;
		private IBusinessRulesForPersonalAccountUpdate _businessRulesForPersonalAccountUpdate;
	    private ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;
		private IScheduleTagAssembler _scheduleTagAssembler;
		private IScheduleSaveHandler _scheduleSaveHandler;

	    [SetUp]
		public void Setup()
		{
			_mock = new MockRepository();
			_dateTimePeriodMock = _mock.StrictMock<IAssembler<DateTimePeriod, DateTimePeriodDto>>();
			_activityRepository = _mock.StrictMock<IActivityRepository>();
			_scheduleStorage = _mock.StrictMock<IScheduleStorage>();
			_personRepository = _mock.StrictMock<IPersonRepository>();
			_scenarioRepository = _mock.StrictMock<IScenarioRepository>();
            _unitOfWorkFactory = _mock.StrictMock<IUnitOfWorkFactory>();
            _currentUnitOfWorkFactory = _mock.DynamicMock<ICurrentUnitOfWorkFactory>();
			_businessRulesForPersonalAccountUpdate = _mock.DynamicMock<IBusinessRulesForPersonalAccountUpdate>();
			_scheduleTagAssembler = _mock.DynamicMock<IScheduleTagAssembler>();
			_scheduleSaveHandler = _mock.DynamicMock<IScheduleSaveHandler>();

			_person = PersonFactory.CreatePerson();
			_person.SetId(Guid.NewGuid());

			_activity = ActivityFactory.CreateActivity("Test Activity");
			_activity.SetId(Guid.NewGuid());

			_scenario = ScenarioFactory.CreateScenarioAggregate();
			_period = _dateOnlyPeriod.ToDateTimePeriod(_person.PermissionInformation.DefaultTimeZone());
			_target = new AddPersonalActivityCommandHandler(_dateTimePeriodMock, _activityRepository, _scheduleStorage, _personRepository, _scenarioRepository, _currentUnitOfWorkFactory, _businessRulesForPersonalAccountUpdate, _scheduleTagAssembler, _scheduleSaveHandler);

			_addPersonalActivityCommand = new AddPersonalActivityCommandDto
			                              	{
			                              		ActivityId = _activity.Id.GetValueOrDefault(),
			                              		Date = _dateOnyldto,
			                              		Period = _periodDto,
			                              		PersonId = _person.Id.GetValueOrDefault(),
                                                ScheduleTagId = Guid.NewGuid() 
			                              	};
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void ShouldAddPersonalActivityInTheDictionarySuccessfully()
		{
			var unitOfWork = _mock.DynamicMock<IUnitOfWork>();
			var scheduleRangeMock = _mock.DynamicMock<IScheduleRange>();
			var scheduleDay = _mock.StrictMock<IScheduleDay>();
			var dictionary = _mock.DynamicMock<IScheduleDictionary>();
			var rules = _mock.DynamicMock<INewBusinessRuleCollection>();
            var scheduleTag = new ScheduleTag() { Description = "test" };
			using (_mock.Record())
			{
				Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
				Expect.Call(_currentUnitOfWorkFactory.Current()).Return(_unitOfWorkFactory);
				Expect.Call(_personRepository.Load(_person.Id.GetValueOrDefault())).Return(_person);
				Expect.Call(_scenarioRepository.LoadDefaultScenario()).Return(_scenario);
				Expect.Call(_scheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(null, null, _dateOnlyPeriod, _scenario)).IgnoreArguments().Return(dictionary);
				Expect.Call(dictionary[_person]).Return(scheduleRangeMock);
				Expect.Call(_activityRepository.Load(_activity.Id.GetValueOrDefault())).Return(_activity);
				Expect.Call(() => scheduleDay.CreateAndAddPersonalActivity(null, new DateTimePeriod())).IgnoreArguments();
				Expect.Call(scheduleRangeMock.ScheduledDay(_startDate)).Return(scheduleDay);
				Expect.Call(_dateTimePeriodMock.DtoToDomainEntity(_periodDto)).Return(_period);
				Expect.Call(_businessRulesForPersonalAccountUpdate.FromScheduleRange(scheduleRangeMock)).Return(rules);
				Expect.Call(_scheduleTagAssembler.DtoToDomainEntity(null)).IgnoreArguments().Return(scheduleTag);
				Expect.Call(() => _scheduleSaveHandler.ProcessSave(scheduleDay, rules, scheduleTag));
			}
			using (_mock.Playback())
			{
				_target.Handle(_addPersonalActivityCommand);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "ForGiven"), Test]
		public void ShouldAddPersonalActivityInTheDictionaryForGivenScenarioSuccessfully()
		{
			var scenarioId = Guid.NewGuid();
			var unitOfWork = _mock.DynamicMock<IUnitOfWork>();
			var scheduleRangeMock = _mock.DynamicMock<IScheduleRange>();
			var scheduleDay = _mock.StrictMock<IScheduleDay>();
			var dictionary = _mock.DynamicMock<IScheduleDictionary>();
			var rules = _mock.DynamicMock<INewBusinessRuleCollection>();
            var scheduleTag = new ScheduleTag() { Description = "test" };
			using (_mock.Record())
			{
				Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
				Expect.Call(_currentUnitOfWorkFactory.Current()).Return(_unitOfWorkFactory);
				Expect.Call(_personRepository.Load(_person.Id.GetValueOrDefault())).Return(_person);
				Expect.Call(_scenarioRepository.Get(scenarioId)).Return(_scenario);
				Expect.Call(_scheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(null, null, _dateOnlyPeriod, _scenario)).IgnoreArguments().Return(dictionary);
				Expect.Call(dictionary[_person]).Return(scheduleRangeMock);
				Expect.Call(_activityRepository.Load(_activity.Id.GetValueOrDefault())).Return(_activity);
				Expect.Call(() => scheduleDay.CreateAndAddPersonalActivity(null, new DateTimePeriod())).IgnoreArguments();
				Expect.Call(scheduleRangeMock.ScheduledDay(_startDate)).Return(scheduleDay);
				Expect.Call(_dateTimePeriodMock.DtoToDomainEntity(_periodDto)).Return(_period);
				Expect.Call(_businessRulesForPersonalAccountUpdate.FromScheduleRange(scheduleRangeMock)).Return(rules);
				Expect.Call(_scheduleTagAssembler.DtoToDomainEntity(null)).IgnoreArguments().Return(scheduleTag);
				Expect.Call(() => _scheduleSaveHandler.ProcessSave(scheduleDay, rules, scheduleTag));
			}
			using (_mock.Playback())
			{
				_addPersonalActivityCommand.ScenarioId = scenarioId;
				_target.Handle(_addPersonalActivityCommand);
			}
		}
	}
}