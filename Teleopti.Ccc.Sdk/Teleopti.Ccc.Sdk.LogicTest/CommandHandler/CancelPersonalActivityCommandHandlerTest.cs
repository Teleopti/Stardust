using System;
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
	public class CancelPersonalActivityCommandHandlerTest
	{
		private MockRepository _mock;
		private IAssembler<DateTimePeriod, DateTimePeriodDto> _dateTimePeriodAssembler;
		private IScheduleRepository _scheduleRepository;
		private IPersonRepository _personRepository;
		private IScenarioRepository _scenarioRepository;
		private IUnitOfWorkFactory _unitOfWorkFactory;
		private ISaveSchedulePartService _saveSchedulePartService;
		private IMessageBrokerEnablerFactory _messageBrokerEnablerFactory;
		private CancelPersonalActivityCommandHandler _target;
		private IPerson _person;
		private Activity _activity;
		private IScenario _scenario;
		private static DateTime _startDate = new DateTime(2012, 1, 1, 0, 0, 0, DateTimeKind.Utc);
		private readonly DateOnlyDto _dateOnlydto = new DateOnlyDto { DateTime = new DateOnly(_startDate) };
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
			_saveSchedulePartService = _mock.DynamicMock<ISaveSchedulePartService>();
			_messageBrokerEnablerFactory = _mock.DynamicMock<IMessageBrokerEnablerFactory>();
			_businessRulesForPersonalAccountUpdate = _mock.DynamicMock<IBusinessRulesForPersonalAccountUpdate>();

			_target = new CancelPersonalActivityCommandHandler(_dateTimePeriodAssembler, _scheduleRepository, _personRepository, _scenarioRepository, _currentUnitOfWorkFactory, _saveSchedulePartService, _messageBrokerEnablerFactory, _businessRulesForPersonalAccountUpdate);

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
			scheduleDay.PersonAssignmentCollection()[0].AddPersonalShift(PersonalShiftFactory.CreatePersonalShift(_activity, _period));
			var scheduleRangeMock = _mock.DynamicMock<IScheduleRange>();
			var dictionary = _mock.DynamicMock<IScheduleDictionary>();
			var rules = _mock.DynamicMock<INewBusinessRuleCollection>();
            
			using (_mock.Record())
			{
				Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
				Expect.Call(_currentUnitOfWorkFactory.LoggedOnUnitOfWorkFactory()).Return(_unitOfWorkFactory);
				Expect.Call(_personRepository.Load(_cancelPersonalActivityCommandDto.PersonId)).Return(_person);
				Expect.Call(_scenarioRepository.LoadDefaultScenario()).Return(_scenario);
				Expect.Call(_dateTimePeriodAssembler.DtoToDomainEntity(_cancelPersonalActivityCommandDto.Period)).Return(_period);
				Expect.Call(_scheduleRepository.FindSchedulesOnlyInGivenPeriod(null, null, _period, _scenario)).
					IgnoreArguments().Return(dictionary);
				Expect.Call(dictionary[_person]).Return(scheduleRangeMock);
				Expect.Call(scheduleRangeMock.ScheduledDay(new DateOnly(_startDate))).Return(scheduleDay);
				Expect.Call(_businessRulesForPersonalAccountUpdate.FromScheduleRange(scheduleRangeMock)).Return(rules);
				Expect.Call(() => _saveSchedulePartService.Save(scheduleDay, rules));
			}
			using (_mock.Playback())
			{
				_target.Handle(_cancelPersonalActivityCommandDto);
				scheduleDay.PersonAssignmentCollection()[0].PersonalShiftCollection.Count.Should().Be.EqualTo(0);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "ForGiven"), Test]
		public void ShouldCancelPersonalActivitySuccessfullyForGivenScenario()
		{
			var scenarioId = Guid.NewGuid();
			var unitOfWork = _mock.DynamicMock<IUnitOfWork>();
			var scheduleDay = _schedulePartFactoryForDomain.CreatePartWithMainShift();
			scheduleDay.PersonAssignmentCollection()[0].AddPersonalShift(PersonalShiftFactory.CreatePersonalShift(_activity, _period));
			var scheduleRangeMock = _mock.DynamicMock<IScheduleRange>();
			var dictionary = _mock.DynamicMock<IScheduleDictionary>();
			var rules = _mock.DynamicMock<INewBusinessRuleCollection>();
            
			using (_mock.Record())
			{
				Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
				Expect.Call(_currentUnitOfWorkFactory.LoggedOnUnitOfWorkFactory()).Return(_unitOfWorkFactory);
				Expect.Call(_personRepository.Load(_cancelPersonalActivityCommandDto.PersonId)).Return(_person);
				Expect.Call(_scenarioRepository.Get(scenarioId)).Return(_scenario);
				Expect.Call(_dateTimePeriodAssembler.DtoToDomainEntity(_cancelPersonalActivityCommandDto.Period)).Return(_period);
				Expect.Call(_scheduleRepository.FindSchedulesOnlyInGivenPeriod(null, null, _period, _scenario)).
					IgnoreArguments().Return(dictionary);
				Expect.Call(dictionary[_person]).Return(scheduleRangeMock);
				Expect.Call(scheduleRangeMock.ScheduledDay(new DateOnly(_startDate))).Return(scheduleDay);
				Expect.Call(_businessRulesForPersonalAccountUpdate.FromScheduleRange(scheduleRangeMock)).Return(rules);
				Expect.Call(() => _saveSchedulePartService.Save(scheduleDay, rules));
			}
			using (_mock.Playback())
			{
				_cancelPersonalActivityCommandDto.ScenarioId = scenarioId;
				_target.Handle(_cancelPersonalActivityCommandDto);
				scheduleDay.PersonAssignmentCollection()[0].PersonalShiftCollection.Count.Should().Be.EqualTo(0);
			}
		}
	}
}