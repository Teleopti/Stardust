using System;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Ccc.Sdk.Logic.CommandHandler;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.Sdk.LogicTest.CommandHandler
{
	[TestFixture]
	public class CancelPersonalActivityCommandHandlerTest
	{
		private MockRepository _mock;
		private IAssembler<DateTimePeriod, DateTimePeriodDto> _dateTimePeriodAssembler;
		private IScheduleStorage _scheduleStorage;
		private IPersonRepository _personRepository;
		private IScenarioRepository _scenarioRepository;
		private IUnitOfWorkFactory _unitOfWorkFactory;
		private CancelPersonalActivityCommandHandler _target;
		private IPerson _person;
		private IActivity _activity;
		private IScenario _scenario;
		private static DateTime _startDate = new DateTime(2012, 1, 1, 0, 0, 0, DateTimeKind.Utc);
		private readonly DateOnlyDto _dateOnlydto = new DateOnlyDto { DateTime = _startDate.Date };
		private readonly DateTimePeriodDto _periodDto = new DateTimePeriodDto
		                                                	{
		                                                		UtcStartTime = _startDate,
		                                                		UtcEndTime = _startDate.AddDays(1)
		                                                	};

		private DateTimePeriod _period;
		private SchedulePartFactoryForDomain _schedulePartFactoryForDomain;
		private CancelPersonalActivityCommandDto _cancelPersonalActivityCommandDto;
		private IBusinessRulesForPersonalAccountUpdate _businessRulesForPersonalAccountUpdate;
	    private ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;
		private IScheduleTagAssembler _scheduleTagAssembler;
		private IScheduleSaveHandler _scheduleSaveHandler;


		[SetUp]
		public void Setup()
		{
			_mock = new MockRepository();
			_dateTimePeriodAssembler = _mock.StrictMock<IAssembler<DateTimePeriod, DateTimePeriodDto>>();
			_scheduleStorage = _mock.StrictMock<IScheduleStorage>();
			_personRepository = _mock.StrictMock<IPersonRepository>();
			_scenarioRepository = _mock.StrictMock<IScenarioRepository>();
            _unitOfWorkFactory = _mock.StrictMock<IUnitOfWorkFactory>();
            _currentUnitOfWorkFactory = _mock.DynamicMock<ICurrentUnitOfWorkFactory>();
			_businessRulesForPersonalAccountUpdate = _mock.DynamicMock<IBusinessRulesForPersonalAccountUpdate>();
			_scheduleTagAssembler = _mock.DynamicMock<IScheduleTagAssembler>();
			_scheduleSaveHandler = _mock.DynamicMock<IScheduleSaveHandler>();

			_target = new CancelPersonalActivityCommandHandler(_dateTimePeriodAssembler, _scheduleTagAssembler, _scheduleStorage, _personRepository, _scenarioRepository, _currentUnitOfWorkFactory, _businessRulesForPersonalAccountUpdate, _scheduleSaveHandler);

			_person = PersonFactory.CreatePerson();
			_person.SetId(Guid.NewGuid());

			_activity = ActivityFactory.CreateActivity("Test Activity");
			_activity.SetId(Guid.NewGuid());

			_scenario = ScenarioFactory.CreateScenarioAggregate();
			_period = new DateTimePeriod(_startDate, _startDate.AddDays(1));
			_schedulePartFactoryForDomain = new SchedulePartFactoryForDomain(_person, _scenario, _period, SkillFactory.CreateSkill("Test Skill"));
			_cancelPersonalActivityCommandDto = new CancelPersonalActivityCommandDto
			                                    	{
			                                    		Date = _dateOnlydto,
			                                    		Period = _periodDto,
			                                    		PersonId = _person.Id.GetValueOrDefault()
			                                    	};
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void ShouldCancelPersonalActivitySuccessfully()
		{
			var unitOfWork = _mock.DynamicMock<IUnitOfWork>();
			var scheduleDay = _schedulePartFactoryForDomain.CreatePartWithMainShift();
			scheduleDay.PersonAssignment().AddPersonalActivity(_activity, _period);
			var scheduleRangeMock = _mock.DynamicMock<IScheduleRange>();
			var dictionary = _mock.DynamicMock<IScheduleDictionary>();
			var rules = _mock.DynamicMock<INewBusinessRuleCollection>();
            
			using (_mock.Record())
			{
				Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
				Expect.Call(_currentUnitOfWorkFactory.Current()).Return(_unitOfWorkFactory);
				Expect.Call(_personRepository.Load(_cancelPersonalActivityCommandDto.PersonId)).Return(_person);
				Expect.Call(_scenarioRepository.LoadDefaultScenario()).Return(_scenario);
				Expect.Call(_dateTimePeriodAssembler.DtoToDomainEntity(_cancelPersonalActivityCommandDto.Period)).Return(_period);
				Expect.Call(_scheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(null, null, new DateOnlyPeriod(), _scenario)).
					IgnoreArguments().Return(dictionary);
				Expect.Call(dictionary[_person]).Return(scheduleRangeMock);
				Expect.Call(scheduleRangeMock.ScheduledDay(new DateOnly(_startDate))).Return(scheduleDay);
				Expect.Call(_businessRulesForPersonalAccountUpdate.FromScheduleRange(scheduleRangeMock)).Return(rules);
				Expect.Call(() => _scheduleSaveHandler.ProcessSave(scheduleDay, rules, null));
			}
			using (_mock.Playback())
			{
				_target.Handle(_cancelPersonalActivityCommandDto);
				scheduleDay.PersonAssignment().PersonalActivities().Should().Be.Empty();
			}
		}

		[Test]
		public void ShouldCancelMeetingPersonalActivitySuccessfully()
		{
			var unitOfWork = _mock.DynamicMock<IUnitOfWork>();
			var scheduleDay = _schedulePartFactoryForDomain.CreatePartWithMainShift();
			scheduleDay.PersonAssignment().AddPersonalActivity(_activity, _period);

			var meetingActivity = ActivityFactory.CreateActivity("Meeting");
			meetingActivity.SetId(Guid.NewGuid());

			scheduleDay.PersonAssignment().AddPersonalActivity(meetingActivity, _period);

			var scheduleRangeMock = _mock.DynamicMock<IScheduleRange>();
			var dictionary = _mock.DynamicMock<IScheduleDictionary>();
			var rules = _mock.DynamicMock<INewBusinessRuleCollection>();

			using (_mock.Record())
			{
				Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
				Expect.Call(_currentUnitOfWorkFactory.Current()).Return(_unitOfWorkFactory);
				Expect.Call(_personRepository.Load(_cancelPersonalActivityCommandDto.PersonId)).Return(_person);
				Expect.Call(_scenarioRepository.LoadDefaultScenario()).Return(_scenario);
				Expect.Call(_dateTimePeriodAssembler.DtoToDomainEntity(_cancelPersonalActivityCommandDto.Period)).Return(_period);
				Expect.Call(_scheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(null, null, new DateOnlyPeriod(), _scenario)).
					IgnoreArguments().Return(dictionary);
				Expect.Call(dictionary[_person]).Return(scheduleRangeMock);
				Expect.Call(scheduleRangeMock.ScheduledDay(new DateOnly(_startDate))).Return(scheduleDay);
				Expect.Call(_businessRulesForPersonalAccountUpdate.FromScheduleRange(scheduleRangeMock)).Return(rules);
				Expect.Call(() => _scheduleSaveHandler.ProcessSave(scheduleDay, rules, null));
			}
			using (_mock.Playback())
			{
				scheduleDay.PersonAssignment().PersonalActivities().Should().Have.Count.EqualTo(2);

				_cancelPersonalActivityCommandDto.ActivityId = meetingActivity.Id;
				
				_target.Handle(_cancelPersonalActivityCommandDto);
				var alala = scheduleDay.PersonAssignment().PersonalActivities().ToList();

				scheduleDay.PersonAssignment().PersonalActivities().Should().Have.Count.EqualTo(1);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "ForGiven"), Test]
		public void ShouldCancelPersonalActivitySuccessfullyForGivenScenario()
		{
			var scenarioId = Guid.NewGuid();
			var unitOfWork = _mock.DynamicMock<IUnitOfWork>();
			var scheduleDay = _schedulePartFactoryForDomain.CreatePartWithMainShift();
			scheduleDay.PersonAssignment().AddPersonalActivity(_activity, _period);
			var scheduleRangeMock = _mock.DynamicMock<IScheduleRange>();
			var dictionary = _mock.DynamicMock<IScheduleDictionary>();
			var rules = _mock.DynamicMock<INewBusinessRuleCollection>();
            
			using (_mock.Record())
			{
				Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
				Expect.Call(_currentUnitOfWorkFactory.Current()).Return(_unitOfWorkFactory);
				Expect.Call(_personRepository.Load(_cancelPersonalActivityCommandDto.PersonId)).Return(_person);
				Expect.Call(_scenarioRepository.Get(scenarioId)).Return(_scenario);
				Expect.Call(_dateTimePeriodAssembler.DtoToDomainEntity(_cancelPersonalActivityCommandDto.Period)).Return(_period);
				Expect.Call(_scheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(null, null, new DateOnlyPeriod(), _scenario)).
					IgnoreArguments().Return(dictionary);
				Expect.Call(dictionary[_person]).Return(scheduleRangeMock);
				Expect.Call(scheduleRangeMock.ScheduledDay(new DateOnly(_startDate))).Return(scheduleDay);
				Expect.Call(_businessRulesForPersonalAccountUpdate.FromScheduleRange(scheduleRangeMock)).Return(rules);
				Expect.Call(() => _scheduleSaveHandler.ProcessSave(scheduleDay, rules, null));
			}
			using (_mock.Playback())
			{
				_cancelPersonalActivityCommandDto.ScenarioId = scenarioId;
				_target.Handle(_cancelPersonalActivityCommandDto);
				scheduleDay.PersonAssignment().PersonalActivities().Should().Be.Empty();
			}
		}
	}
}