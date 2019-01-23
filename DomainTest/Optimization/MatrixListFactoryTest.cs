using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;


namespace Teleopti.Ccc.DomainTest.Optimization
{
	[DomainTest]
	public class MatrixListFactoryTest
	{
		public MatrixListFactory Target;
		public SchedulerStateHolder SchedulerStateHolder;

		[Test]
		public void ForSelectionShouldNotCreateMatrixesOutsideSelectedArea()
		{
			var agent = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue);
			agent.Period(new DateOnly(2015, 10, 12)).PersonContract.Contract.WorkTimeDirective = new WorkTimeDirective(TimeSpan.Zero, TimeSpan.FromHours(54), TimeSpan.Zero, TimeSpan.FromHours(36));
			agent.SetId(Guid.NewGuid());

			var schedulePeriod = new SchedulePeriod(new DateOnly(2015, 10, 12), SchedulePeriodType.Week, 1);
			agent.AddSchedulePeriod(schedulePeriod);

			SchedulerStateHolder.RequestedPeriod =
				new DateOnlyPeriodAsDateTimePeriod(new DateOnlyPeriod(2015, 10, 12, 2015, 10, 25), TimeZoneInfo.Utc);
			SchedulerStateHolder.SchedulingResultState.LoadedAgents.Add(agent);

			var scheduleDictionary = new ScheduleDictionaryForTest(new Scenario("unimportant"), new ScheduleDateTimePeriod(SchedulerStateHolder.RequestedPeriod.Period(), new[] { agent }).VisiblePeriod);
			SchedulerStateHolder.SchedulingResultState.Schedules = scheduleDictionary;

			var scheduleDay = scheduleDictionary[agent].ScheduledDay(new DateOnly(2015, 10, 12));
			var matrixList = Target.CreateMatrixListForSelection(scheduleDictionary, new List <IScheduleDay>{scheduleDay});

			matrixList.Count().Should().Not.Be.GreaterThan(1);

			scheduleDay = scheduleDictionary[agent].ScheduledDay(new DateOnly(2015, 10, 25));
			matrixList = Target.CreateMatrixListForSelection(scheduleDictionary, new List <IScheduleDay> { scheduleDay });

			matrixList.Count().Should().Not.Be.GreaterThan(1);
		}

		[Test]
		public void ShouldCreateMatrixesOutsideOfSelectedArea()
		{
			var agent = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue);
			agent.Period(new DateOnly(2015, 10, 12)).PersonContract.Contract.WorkTimeDirective = new WorkTimeDirective(TimeSpan.Zero, TimeSpan.FromHours(54), TimeSpan.Zero, TimeSpan.FromHours(36));
			agent.SetId(Guid.NewGuid());

			var schedulePeriod = new SchedulePeriod(new DateOnly(2015, 10, 12), SchedulePeriodType.Week, 1);
			agent.AddSchedulePeriod(schedulePeriod);

			SchedulerStateHolder.RequestedPeriod =
				new DateOnlyPeriodAsDateTimePeriod(new DateOnlyPeriod(2015, 10, 12, 2015, 10, 25), TimeZoneInfo.Utc);
			SchedulerStateHolder.SchedulingResultState.LoadedAgents.Add(agent);

			var scheduleDictionary = new ScheduleDictionaryForTest(new Scenario("unimportant"), new ScheduleDateTimePeriod(SchedulerStateHolder.RequestedPeriod.Period(), new[] { agent }).VisiblePeriod);
			SchedulerStateHolder.SchedulingResultState.Schedules = scheduleDictionary;	

			var matrixList = Target.CreateMatrixListAllForLoadedPeriod(scheduleDictionary, SchedulerStateHolder.SchedulingResultState.LoadedAgents, new DateOnlyPeriod(2015, 10, 12, 2015, 10, 12));

			matrixList.Count().Should().Be.GreaterThan(1);
		}

		[Test]
		public void OnlySelectedPeriodShouldBeUnlockedInRelevantMatrix()
		{
			var agent = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue);
			agent.Period(new DateOnly(2015, 10, 12)).PersonContract.Contract.WorkTimeDirective = new WorkTimeDirective(TimeSpan.Zero, TimeSpan.FromHours(54), TimeSpan.Zero, TimeSpan.FromHours(36));
			agent.SetId(Guid.NewGuid());

			var schedulePeriod = new SchedulePeriod(new DateOnly(2015, 10, 12), SchedulePeriodType.Week, 1);
			agent.AddSchedulePeriod(schedulePeriod);

			SchedulerStateHolder.RequestedPeriod =
				new DateOnlyPeriodAsDateTimePeriod(new DateOnlyPeriod(2015, 10, 12, 2015, 10, 25), TimeZoneInfo.Utc);
			SchedulerStateHolder.SchedulingResultState.LoadedAgents.Add(agent);

			var scheduleDictionary = new ScheduleDictionaryForTest(new Scenario("unimportant"), new ScheduleDateTimePeriod(SchedulerStateHolder.RequestedPeriod.Period(), new[] { agent }).VisiblePeriod);
			SchedulerStateHolder.SchedulingResultState.Schedules = scheduleDictionary;

			var matrixList = Target.CreateMatrixListAllForLoadedPeriod(scheduleDictionary, SchedulerStateHolder.SchedulingResultState.LoadedAgents, new DateOnlyPeriod(2015, 10, 12, 2015, 10, 12));

			matrixList.First().UnlockedDays.Count.Should().Be.EqualTo(1);
			matrixList.First().UnlockedDays.First().Day.Should().Be.EqualTo(new DateOnly(2015, 10, 12));
		}

		[Test]
		public void ShouldCreateMatrixesPerPerson()
		{
			var agent1 = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue);
			var agent2 = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue);

			agent1.WithName(new Name("agent_1", "agent_1"));
			agent2.WithName(new Name("agent_2", "agent_2"));

			agent1.Period(new DateOnly(2015, 10 , 4)).PersonContract.Contract.WorkTimeDirective = new WorkTimeDirective(TimeSpan.Zero, TimeSpan.FromHours(54), TimeSpan.Zero, TimeSpan.FromHours(36));
			agent2.Period(new DateOnly(2015, 10, 4)).PersonContract.Contract.WorkTimeDirective = new WorkTimeDirective(TimeSpan.Zero, TimeSpan.FromHours(54), TimeSpan.Zero, TimeSpan.FromHours(36));
			
			agent1.SetId(Guid.NewGuid());
			agent2.SetId(Guid.NewGuid());

			var schedulePeriod1 = new SchedulePeriod(new DateOnly(2016, 1, 4), SchedulePeriodType.Week, 8);
			var schedulePeriod2 = new SchedulePeriod(new DateOnly(2015, 11, 23), SchedulePeriodType.Week, 6);
			
			agent1.AddSchedulePeriod(schedulePeriod1);
			agent2.AddSchedulePeriod(schedulePeriod2);

			SchedulerStateHolder.RequestedPeriod = new DateOnlyPeriodAsDateTimePeriod(new DateOnlyPeriod(2016, 1, 4, 2016, 2, 28), TimeZoneInfo.Utc);
			SchedulerStateHolder.SchedulingResultState.LoadedAgents.Add(agent1);
			SchedulerStateHolder.SchedulingResultState.LoadedAgents.Add(agent2);

			var scheduleDictionary = new ScheduleDictionaryForTest(new Scenario("unimportant"), new ScheduleDateTimePeriod(SchedulerStateHolder.RequestedPeriod.Period(), new[] { agent1, agent2 }).VisiblePeriod);
			SchedulerStateHolder.SchedulingResultState.Schedules = scheduleDictionary;

			var scheduleDay1 = scheduleDictionary[agent1].ScheduledDay(new DateOnly(2016, 1, 4));
			var scheduleDay2 = scheduleDictionary[agent1].ScheduledDay(new DateOnly(2016, 2, 28));
			var scheduleDay3 = scheduleDictionary[agent2].ScheduledDay(new DateOnly(2016, 1, 4));
			var scheduleDay4 = scheduleDictionary[agent2].ScheduledDay(new DateOnly(2016, 2, 28));
			var matrixList = Target.CreateMatrixListForSelection(scheduleDictionary, new List<IScheduleDay> { scheduleDay1, scheduleDay2, scheduleDay3, scheduleDay4}).ToList();

			matrixList.Count.Should().Be.EqualTo(3);
			matrixList[0].Person.Should().Be.EqualTo(agent1);
			matrixList[1].Person.Should().Be.EqualTo(agent2);
			matrixList[2].Person.Should().Be.EqualTo(agent2);
		}
	}
}