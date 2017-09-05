using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.SaveSchedulePart;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Staffing;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Ccc.Sdk.Logic.CommandHandler;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.LogicTest.CommandHandler
{
	[TestFixture]
	public class AddPersonalActivityCommandHandlerTest
	{
		private IAssembler<DateTimePeriod, DateTimePeriodDto> _dateTimePeriodMock;
		private IActivityRepository _activityRepository;
		private IScheduleStorage _scheduleStorage;
		private IPersonRepository _personRepository;
		private IScenarioRepository _scenarioRepository;
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
		private ScheduleTag _scheduleTag;

		[SetUp]
		public void Setup()
		{
			var personAbsenceAccountRepository = new FakePersonAbsenceAccountRepository();
			var scheduleTagRepository = new FakeScheduleTagRepository();

			_scheduleTag = new ScheduleTag{Description = "Manual"}.WithId();
			_dateTimePeriodMock = new DateTimePeriodAssembler();
			_activityRepository = new FakeActivityRepository();
			_scheduleStorage = new FakeScheduleDataReadScheduleStorage();
			_personRepository = new FakePersonRepository(new FakeStorage());
			_scenarioRepository = new FakeScenarioRepository();
            _currentUnitOfWorkFactory = new FakeCurrentUnitOfWorkFactory();
			_scheduleTagAssembler = new ScheduleTagAssembler(scheduleTagRepository);
			_scheduleSaveHandler = new ScheduleSaveHandler(new SaveSchedulePartService(
				new FakeScheduleDifferenceSaver(_scheduleStorage, new EmptyScheduleDayDifferenceSaver()),
				personAbsenceAccountRepository, new DoNothingScheduleDayChangeCallBack()));
			_businessRulesForPersonalAccountUpdate = new BusinessRulesForPersonalAccountUpdate(personAbsenceAccountRepository, new FakeSchedulingResultStateHolder());
			scheduleTagRepository.Add(_scheduleTag);

			_person = PersonFactory.CreatePerson().WithId();
			_activity = ActivityFactory.CreateActivity("Test Activity").WithId();
			
			_scenario = ScenarioFactory.CreateScenarioAggregate("Default",true);
			_period = _dateOnlyPeriod.ToDateTimePeriod(_person.PermissionInformation.DefaultTimeZone());
			_target = new AddPersonalActivityCommandHandler(_dateTimePeriodMock, _activityRepository, _scheduleStorage, _personRepository, _scenarioRepository, _currentUnitOfWorkFactory, _businessRulesForPersonalAccountUpdate, _scheduleTagAssembler, _scheduleSaveHandler);

			_addPersonalActivityCommand = new AddPersonalActivityCommandDto
			                              	{
			                              		ActivityId = _activity.Id.GetValueOrDefault(),
			                              		Date = _dateOnyldto,
			                              		Period = _periodDto,
			                              		PersonId = _person.Id.GetValueOrDefault(),
                                                ScheduleTagId = _scheduleTag.Id
			                              	};
		}

		[Test]
		public void ShouldAddPersonalActivityInTheDictionarySuccessfully()
		{
			_personRepository.Add(_person);
			_scenarioRepository.Add(_scenario);
			_activityRepository.Add(_activity);
			
			_target.Handle(_addPersonalActivityCommand);

			_scheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(_person, new ScheduleDictionaryLoadOptions(false, false),
					_period, _scenario)[_person].ScheduledDay(_startDate).PersonAssignment().PersonalActivities().Count().Should().Be
				.EqualTo(1);
		}

		[Test]
		public void ShouldAddPersonalActivityInTheDictionaryForGivenScenarioSuccessfully()
		{
			var scenario = ScenarioFactory.CreateScenario("High", false, false).WithId();

			_personRepository.Add(_person);
			_scenarioRepository.Add(scenario);
			_activityRepository.Add(_activity);

			_addPersonalActivityCommand.ScenarioId = scenario.Id;
			_target.Handle(_addPersonalActivityCommand);

			_scheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(_person, new ScheduleDictionaryLoadOptions(false, false),
					_period, scenario)[_person].ScheduledDay(_startDate).PersonAssignment().PersonalActivities().Count().Should().Be
				.EqualTo(1);
		}

		[Test]
		public void ShouldKeepOldTagForDayWhenChangedAndNoTagSpecified()
		{
			var agentDayScheduleTag = new AgentDayScheduleTag(_person, _startDate, _scenario, _scheduleTag).WithId();

			_personRepository.Add(_person);
			_scenarioRepository.Add(_scenario);
			_activityRepository.Add(_activity);
			_scheduleStorage.Add(agentDayScheduleTag);
			_addPersonalActivityCommand.ScheduleTagId = null;
			
			_target.Handle(_addPersonalActivityCommand);

			_scheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(_person, new ScheduleDictionaryLoadOptions(false, false),
					_period, _scenario)[_person].ScheduledDay(_startDate).ScheduleTag().Should().Be.EqualTo(_scheduleTag);
		}
	}
}