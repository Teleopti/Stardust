using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.SaveSchedulePart;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Ccc.Sdk.Logic.CommandHandler;
using Teleopti.Ccc.Sdk.WcfHost.Ioc;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.LogicTest.CommandHandler
{
	[TestFixture]
	[DomainTest]
	public class AddPersonalActivityCommandHandlerTest : IExtendSystem, IIsolateSystem
	{
		public FakeActivityRepository ActivityRepository;
		public IScheduleStorage ScheduleStorage;
		public FakePersonRepository PersonRepository;
		public FakeScenarioRepository ScenarioRepository;
		public FakeScheduleTagRepository ScheduleTagRepository;
		public FakeAgentDayScheduleTagRepository AgentDayScheduleTagRepository;
		public AddPersonalActivityCommandHandler Target;

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
		private ScheduleTag _scheduleTag;

		[SetUp]
		public void Setup()
		{
			_scheduleTag = new ScheduleTag{Description = "Manual"}.WithId();
			
			_person = PersonFactory.CreatePerson().WithId();
			_activity = ActivityFactory.CreateActivity("Test Activity").WithId();
			
			_scenario = ScenarioFactory.CreateScenarioAggregate("Default",true);
			_period = _dateOnlyPeriod.ToDateTimePeriod(_person.PermissionInformation.DefaultTimeZone());
			
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
			ScheduleTagRepository.Add(_scheduleTag);
			PersonRepository.Add(_person);
			ScenarioRepository.Add(_scenario);
			ActivityRepository.Add(_activity);
			
			Target.Handle(_addPersonalActivityCommand);

			ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(_person, new ScheduleDictionaryLoadOptions(false, false),
					_period, _scenario)[_person].ScheduledDay(_startDate).PersonAssignment().PersonalActivities().Count().Should().Be
				.EqualTo(1);
		}

		[Test]
		public void ShouldAddPersonalActivityInTheDictionaryForGivenScenarioSuccessfully()
		{
			var scenario = ScenarioFactory.CreateScenario("High", false, false).WithId();

			ScheduleTagRepository.Add(_scheduleTag);
			PersonRepository.Add(_person);
			ScenarioRepository.Add(scenario);
			ActivityRepository.Add(_activity);

			_addPersonalActivityCommand.ScenarioId = scenario.Id;
			Target.Handle(_addPersonalActivityCommand);

			ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(_person, new ScheduleDictionaryLoadOptions(false, false),
					_period, scenario)[_person].ScheduledDay(_startDate).PersonAssignment().PersonalActivities().Count().Should().Be
				.EqualTo(1);
		}

		[Test]
		public void ShouldKeepOldTagForDayWhenChangedAndNoTagSpecified()
		{
			var agentDayScheduleTag = new AgentDayScheduleTag(_person, _startDate, _scenario, _scheduleTag).WithId();

			ScheduleTagRepository.Add(_scheduleTag);
			PersonRepository.Add(_person);
			ScenarioRepository.Add(_scenario);
			ActivityRepository.Add(_activity);
			AgentDayScheduleTagRepository.Add(agentDayScheduleTag);

			_addPersonalActivityCommand.ScheduleTagId = null;
			
			Target.Handle(_addPersonalActivityCommand);

			ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(_person, new ScheduleDictionaryLoadOptions(false, false),
					_period, _scenario)[_person].ScheduledDay(_startDate).ScheduleTag().Should().Be.EqualTo(_scheduleTag);
		}
		
		public void Extend(IExtend extend, IIocConfiguration configuration)
		{
			extend.AddService<AddPersonalActivityCommandHandler>();
			extend.AddService<ScheduleSaveHandler>();
			extend.AddService<SaveSchedulePartService>();
			extend.AddModule(new AssemblerModule());
		}

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<FakeDataSourcesFactoryNoEvents>().For<IDataSourcesFactory>();
		}
	}
}