using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Logon;
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


namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{
	[DomainTest]
	public class SwapAndModifyServiceNewByFakeTest : IIsolateSystem, IExtendSystem
	{
		public SwapAndModifyServiceNew Target;
		public FakePersonAssignmentRepository PersonAssignmentRepository;
		public FakeSkillRepository SkillRepository;
		public FakePersonRepository PersonRepository;
		public FakeScenarioRepository ScenarioRepository;
		public FakeActivityRepository ActivityRepository;
		public INow Now;
		public ScheduleChangedEventDetector ScheduleChangedEventDetector;
		
		public void Extend(IExtend extend, IocConfiguration configuration)
		{
			extend.AddService<SwapAndModifyServiceNew>();
		}

		public void Isolate(IIsolate isolate)
		{				
			isolate.UseTestDouble<ScheduleChangedEventDetector>().For<IHandleEvent<ScheduleChangedEvent>>();
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
			agent1Assignment.PopAllEvents(null);

			var scheduleDictionary = ScheduleDictionaryForTest.WithPersonAssignment(scenario,date.Date,agent1Assignment, new FullPermission());

			Target.Swap(agent1,agent2,
				new List<DateOnly> { date },new List<DateOnly>(),scheduleDictionary,NewBusinessRuleCollection.Minimum(),new ScheduleTagSetter(NullScheduleTag.Instance));

			var events = scheduleDictionary[agent1].ScheduledDay(date).PersonAssignment().PopAllEvents(null).ToList();
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
			agent1Assignment.PopAllEvents(null);

			var agent2Assignment = PersonAssignmentRepository.GetSingle(date,agent2) as PersonAssignment;
			var scheduleDictionary = ScheduleDictionaryForTest.WithScheduleDataForManyPeople(scenario, date.ToDateTimePeriod(TimeZoneInfo.Local), new FullPermission(), agent1Assignment, agent2Assignment);
			
			Target.Swap(agent1,agent2,
				new List<DateOnly> { date },new List<DateOnly>(),scheduleDictionary,NewBusinessRuleCollection.Minimum(),new ScheduleTagSetter(NullScheduleTag.Instance));

			var events = scheduleDictionary[agent1].ScheduledDay(date).PersonAssignment().PopAllEvents(null).ToList();
			events.Any(e => e.GetType() == typeof(ActivityAddedEvent)).Should().Be.True();
		}
	}
}