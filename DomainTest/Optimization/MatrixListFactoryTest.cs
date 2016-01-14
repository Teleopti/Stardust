using System;
using System.Collections.Generic;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization
{
	[DomainTest]
	public class MatrixListFactoryTest
	{
		public IMatrixListFactory Target;
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
			var loadedPeriod = new DateOnlyPeriodAsDateTimePeriod(new DateOnlyPeriod(2015, 9, 1, 2015, 11, 30), TimeZoneInfo.Utc);
			SchedulerStateHolder.SetLoadedPeriod_UseOnlyFromTest_ShouldProbablyBePutOnScheduleDictionaryInsteadIfNeededAtAll(loadedPeriod.Period());
			SchedulerStateHolder.FilterPersons(new[] { agent });
			SchedulerStateHolder.SchedulingResultState.PersonsInOrganization.Add(agent);

			var scheduleDictionary = new ScheduleDictionaryForTest(new Scenario("unimportant"), new ScheduleDateTimePeriod(SchedulerStateHolder.RequestedPeriod.Period(), new[] { agent }).VisiblePeriod);
			SchedulerStateHolder.SchedulingResultState.Schedules = scheduleDictionary;

			var scheduleDay = scheduleDictionary[agent].ScheduledDay(new DateOnly(2015, 10, 12));
			var matrixList = Target.CreateMatrixListForSelection(new List<IScheduleDay>{scheduleDay});

			matrixList.Count.Should().Not.Be.GreaterThan(1);

			scheduleDay = scheduleDictionary[agent].ScheduledDay(new DateOnly(2015, 10, 25));
			matrixList = Target.CreateMatrixListForSelection(new List<IScheduleDay> { scheduleDay });

			matrixList.Count.Should().Not.Be.GreaterThan(1);
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
			var loadedPeriod = new DateOnlyPeriodAsDateTimePeriod(new DateOnlyPeriod(2015, 9, 1, 2015, 11, 30), TimeZoneInfo.Utc);
			SchedulerStateHolder.SetLoadedPeriod_UseOnlyFromTest_ShouldProbablyBePutOnScheduleDictionaryInsteadIfNeededAtAll(loadedPeriod.Period());
			SchedulerStateHolder.FilterPersons(new[] { agent });
			SchedulerStateHolder.SchedulingResultState.PersonsInOrganization.Add(agent);

			var scheduleDictionary = new ScheduleDictionaryForTest(new Scenario("unimportant"), new ScheduleDateTimePeriod(SchedulerStateHolder.RequestedPeriod.Period(), new[] { agent }).VisiblePeriod);
			SchedulerStateHolder.SchedulingResultState.Schedules = scheduleDictionary;	

			var matrixList = Target.CreateMatrixListAllForLoadedPeriod(new DateOnlyPeriod(2015, 10, 12, 2015, 10, 12));

			matrixList.Count.Should().Be.GreaterThan(1);
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
			var loadedPeriod = new DateOnlyPeriodAsDateTimePeriod(new DateOnlyPeriod(2015, 9, 1, 2015, 11, 30), TimeZoneInfo.Utc);
			SchedulerStateHolder.SetLoadedPeriod_UseOnlyFromTest_ShouldProbablyBePutOnScheduleDictionaryInsteadIfNeededAtAll(loadedPeriod.Period());
			SchedulerStateHolder.FilterPersons(new[] { agent });
			SchedulerStateHolder.SchedulingResultState.PersonsInOrganization.Add(agent);

			var scheduleDictionary = new ScheduleDictionaryForTest(new Scenario("unimportant"), new ScheduleDateTimePeriod(SchedulerStateHolder.RequestedPeriod.Period(), new[] { agent }).VisiblePeriod);
			SchedulerStateHolder.SchedulingResultState.Schedules = scheduleDictionary;

			var matrixList = Target.CreateMatrixListAllForLoadedPeriod(new DateOnlyPeriod(2015, 10, 12, 2015, 10, 12));

			matrixList[0].UnlockedDays.Count.Should().Be.EqualTo(1);
			matrixList[0].UnlockedDays[0].Day.Should().Be.EqualTo(new DateOnly(2015, 10, 12));
		}
	}
}