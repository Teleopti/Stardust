using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.DomainTest.Common;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{
	[DomainTest]
	[TestFixture]
	[UseEventPublisher(typeof(FakeEventPublisherWithOverwritingHandlers))]
	public class SwapAndModifyServiceNewByFakeTest : ISetup
	{
		public SwapAndModifyServiceNew Target;
		public FakePersonAssignmentRepository PersonAssignmentRepository;
		public FakeSkillRepository SkillRepository;
		public FakePersonRepository PersonRepository;
		public FakeScenarioRepository ScenarioRepository;
		public FakeActivityRepository ActivityRepository;
		public INow Now;
		public FakeEventPublisherWithOverwritingHandlers EventPublisher;		
		public ScheduleChangedEventDetector ScheduleChangedEventDetector;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{				
			system.UseTestDouble<ScheduleChangedEventDetector>().For<IHandleEvent<ScheduleChangedEvent>>();
			system.AddService<SwapAndModifyServiceNew>();
		}

		[Test]
		public void TargetShouldBeDefined()
		{
			Target.Should().Not.Be.Null();		
		}

		[Test]
		public void ShouldFirePersonAssignmentLayerRemovedEventWhenSwappingWithEmptyDay()
		{
			var date = new DateOnly(2016,4,8);

			var activity = ActivityRepository.Has("_");
			var skill = SkillRepository.Has("skill",activity);
			var schedulePeriod = new SchedulePeriod(date,SchedulePeriodType.Week,1);
			var scenario = ScenarioRepository.Has("some name");
			var shiftCategory = new ShiftCategory("_").WithId();

			var agent1 = PersonRepository.Has(new Contract("_"),new ContractSchedule("_"),new PartTimePercentage("_"),new Team { Site = new Site("site") },schedulePeriod,skill);
			var agent2 = PersonRepository.Has(new Contract("_"),new ContractSchedule("_"),new PartTimePercentage("_"),new Team { Site = new Site("site") },schedulePeriod,skill);

			PersonAssignmentRepository.Has(agent1,scenario,activity,shiftCategory,new DateOnlyPeriod(date,date),
				new TimePeriod(8,0,16,0));

			var agent1Assignment = PersonAssignmentRepository.GetSingle(date,agent1) as PersonAssignment;
			var scheduleDictionary = ScheduleDictionaryForTest.WithPersonAssignment(scenario,date.Date,agent1Assignment);

			agent1Assignment.PopAllEvents(Now);
					
			Target.Swap(agent1,agent2,
				new List<DateOnly> { date },new List<DateOnly>(),scheduleDictionary,NewBusinessRuleCollection.Minimum(),new ScheduleTagSetter(NullScheduleTag.Instance));

			var events = agent1Assignment.PopAllEvents(Now).ToList();
			events.Any(e => e.GetType() == typeof(PersonAssignmentLayerRemovedEvent)).Should().Be.True();			
		}

		[Test]
		public void ShouldFireActivityAddedEventWhenSwappingWithShiftDay()
		{
			var date = new DateOnly(2016,4,8);

			var activity = ActivityRepository.Has("_");
			var skill = SkillRepository.Has("skill",activity);
			var schedulePeriod = new SchedulePeriod(date,SchedulePeriodType.Week,1);
			var scenario = ScenarioRepository.Has("some name");
			var shiftCategory = new ShiftCategory("_").WithId();

			var agent1 = PersonRepository.Has(new Contract("_"),new ContractSchedule("_"),new PartTimePercentage("_"),new Team { Site = new Site("site") },schedulePeriod,skill);
			var agent2 = PersonRepository.Has(new Contract("_"),new ContractSchedule("_"),new PartTimePercentage("_"),new Team { Site = new Site("site") },schedulePeriod,skill);

			PersonAssignmentRepository.Has(agent1,scenario,activity,shiftCategory,new DateOnlyPeriod(date,date),
				new TimePeriod(10,0,12,0));
			PersonAssignmentRepository.Has(agent2,scenario,activity,shiftCategory,new DateOnlyPeriod(date,date),
				new TimePeriod(15,0,20,0));

			var agent1Assignment = PersonAssignmentRepository.GetSingle(date,agent1) as PersonAssignment;
			var agent2Assignment = PersonAssignmentRepository.GetSingle(date,agent2) as PersonAssignment;
			var scheduleDictionary = ScheduleDictionaryForTest.WithScheduleDataForManyPeople(scenario,
				(new DateOnlyPeriod(date,date)).ToDateTimePeriod(TimeZoneInfo.Local), new [] { agent1Assignment ,agent2Assignment } );
			
			agent1Assignment.PopAllEvents(Now);

			Target.Swap(agent1,agent2,
				new List<DateOnly> { date },new List<DateOnly>(),scheduleDictionary,NewBusinessRuleCollection.Minimum(),new ScheduleTagSetter(NullScheduleTag.Instance));

			var events = agent1Assignment.PopAllEvents(Now).ToList();
			events.Any(e => e.GetType() == typeof(ActivityAddedEvent)).Should().Be.True();
		}


		[Test]
		public void ShouldEventuallyFireScheduleChangedEventWhenSwappingWithEmptyDay()
		{						
			var date = new DateOnly(2016, 4, 8);

			var activity = ActivityRepository.Has("_");
			var skill = SkillRepository.Has("skill", activity);
			var schedulePeriod = new SchedulePeriod(date, SchedulePeriodType.Week, 1);
			var scenario = ScenarioRepository.Has("some name");
			var shiftCategory = new ShiftCategory("_").WithId();

			var agent1= PersonRepository.Has(new Contract("_"), new ContractSchedule("_"), new PartTimePercentage("_"), new Team { Site = new Site("site") }, schedulePeriod, skill);
			var agent2= PersonRepository.Has(new Contract("_"), new ContractSchedule("_"), new PartTimePercentage("_"), new Team { Site = new Site("site") }, schedulePeriod, skill);

			PersonAssignmentRepository.Has(agent1, scenario, activity, shiftCategory, new DateOnlyPeriod(date, date),
				new TimePeriod(8, 0, 16, 0));

			var agent1Assignment = PersonAssignmentRepository.GetSingle(date, agent1) as PersonAssignment;
			var scheduleDictionary = ScheduleDictionaryForTest.WithPersonAssignment(scenario, date.Date, agent1Assignment);

			agent1Assignment.PopAllEvents(Now);
			EventPublisher.OverwriteHandler(typeof(ScheduleChangedEvent),typeof(ScheduleChangedEventDetector));
			ScheduleChangedEventDetector.Reset();

			Target.Swap( agent1, agent2,
				new List<DateOnly> { date }, new List<DateOnly>(), scheduleDictionary,NewBusinessRuleCollection.Minimum(),new ScheduleTagSetter(NullScheduleTag.Instance));

			var events = agent1Assignment.PopAllEvents(Now).ToList();		
			events.ForEach(e => EventPublisher.Publish(e));
			ScheduleChangedEventDetector.GetEvents().Any(e => e.PersonId == agent1.Id).Should().Be.True();
		}

		[Test]
		public void ShouldFireEventsWithCorrectTrackedCommandInfoWhenSwapWithDayOff()
		{
			var date = new DateOnly(2016,4,8);

			var activity = ActivityRepository.Has("_");
			var skill = SkillRepository.Has("skill",activity);
			var schedulePeriod = new SchedulePeriod(date,SchedulePeriodType.Week,1);
			var scenario = ScenarioRepository.Has("some name");
			var shiftCategory = new ShiftCategory("_").WithId();
			var dayOffTemplate = new DayOffTemplate(new Description("_"));
			dayOffTemplate.SetTargetAndFlexibility(TimeSpan.FromHours(36),TimeSpan.FromHours(6));
			dayOffTemplate.Anchor = TimeSpan.FromHours(12.5);

			var agent1 = PersonRepository.Has(new Contract("_"),new ContractSchedule("_"),new PartTimePercentage("_"),new Team { Site = new Site("site") },schedulePeriod,skill);
			var agent2 = PersonRepository.Has(new Contract("_"),new ContractSchedule("_"),new PartTimePercentage("_"),new Team { Site = new Site("site") },schedulePeriod,skill);

			PersonAssignmentRepository.Has(agent1,scenario,activity,shiftCategory,new DateOnlyPeriod(date,date),
				new TimePeriod(8,0,16,0));
			PersonAssignmentRepository.Has(agent1,scenario,dayOffTemplate, date.AddDays(1));

			var assignments = PersonAssignmentRepository.LoadAll().ToArray();
			var agent1Assignment = assignments[0] as PersonAssignment;  //PersonAssignmentRepository.GetSingle(date,agent1) as PersonAssignment;
			var agent2Assignment = assignments[1] as PersonAssignment;
			var scheduleDictionary = ScheduleDictionaryForTest.WithPersonAssignment(scenario,date.Date,agent1Assignment);
			(scheduleDictionary as ScheduleDictionaryForTest).AddPersonAssignment((agent2Assignment));


			agent1Assignment.PopAllEvents(Now);
			agent2Assignment.PopAllEvents(Now);
			EventPublisher.OverwriteHandler(typeof(ScheduleChangedEvent),typeof(ScheduleChangedEventDetector));
			ScheduleChangedEventDetector.Reset();

			var trackedCommandInfo = new TrackedCommandInfo
			{
				OperatedPersonId = Guid.NewGuid(),
				TrackId = Guid.NewGuid()
			};

			Target.Swap(agent1,agent2,
				new List<DateOnly> { date },new List<DateOnly>(),scheduleDictionary,NewBusinessRuleCollection.Minimum(),new ScheduleTagSetter(NullScheduleTag.Instance),trackedCommandInfo);

			var eventsFromAgent1 = agent1Assignment.PopAllEvents(Now).ToList();
			var eventsFromAgent2 = agent2Assignment.PopAllEvents(Now).ToList();

			eventsFromAgent1.ForEach(e => EventPublisher.Publish(e));
			eventsFromAgent2.ForEach(e => EventPublisher.Publish(e));
			ScheduleChangedEventDetector.GetEvents().Should().Have.Count.GreaterThan(0);
			ScheduleChangedEventDetector.GetEvents().All(e => e.CommandId == trackedCommandInfo.TrackId).Should().Be.True();
		}
	}
}