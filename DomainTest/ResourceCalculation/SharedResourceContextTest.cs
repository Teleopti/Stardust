using System;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.DomainTest.SchedulingScenarios;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;
using Task = System.Threading.Tasks.Task;

namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{
	[DomainTest]
	[Toggle(Toggles.ResourcePlanner_SpeedUpManualChanges_37029)]
	public class SharedResourceContextTest
	{
		public ISharedResourceContext Target;
		public Func<ISchedulerStateHolder> SchedulerStateHolder;
		public IResourceOptimizationHelperExtended ResourceOptimizationHelperExtended;
		public IScheduleDayChangeCallback ScheduleDayChangeCallback;

		[Test]
		public void ShouldMakeSureContextIsAlive()
		{
			SchedulerStateHolder.Fill(new Scenario("_"), DateOnly.Today.ToDateOnlyPeriod(), Enumerable.Empty<IPerson>(), Enumerable.Empty<IPersistableScheduleData>(), Enumerable.Empty<ISkillDay>());

			using (Target.MakeSureExists(new DateOnlyPeriod(), false)) { }

			ResourceCalculationContext.InContext
				.Should().Be.True();
		}

		[Test]
		public void ShouldNotCreateNewIfAlreadyExist()
		{
			SchedulerStateHolder.Fill(new Scenario("_"), DateOnly.Today.ToDateOnlyPeriod(), Enumerable.Empty<IPerson>(), Enumerable.Empty<IPersistableScheduleData>(), Enumerable.Empty<ISkillDay>());

			Target.MakeSureExists(new DateOnlyPeriod(), false);
			var context = ResourceCalculationContext.Fetch();
			Target.MakeSureExists(new DateOnlyPeriod(), false);

			ResourceCalculationContext.Fetch()
				.Should().Be.SameInstanceAs(context);
		}

		[Test]
		[Repeat(25)] //to make sure it works if/when threads are reused
		public void ShouldShareBetweenThreads()
		{
			SchedulerStateHolder.Fill(new Scenario("_"), DateOnly.Today.ToDateOnlyPeriod(), Enumerable.Empty<IPerson>(), Enumerable.Empty<IPersistableScheduleData>(), Enumerable.Empty<ISkillDay>());

			Target.MakeSureExists(new DateOnlyPeriod(), false);
			var context = ResourceCalculationContext.Fetch();
			Task.Factory.StartNew(() =>
			{
				Target.MakeSureExists(new DateOnlyPeriod(), false);
				ResourceCalculationContext.Fetch()
					.Should().Be.SameInstanceAs(context);
			}).Wait();
		}

		[Test]
		public void ShouldForceNewContext()
		{
			SchedulerStateHolder.Fill(new Scenario("_"), DateOnly.Today.ToDateOnlyPeriod(), Enumerable.Empty<IPerson>(), Enumerable.Empty<IPersistableScheduleData>(), Enumerable.Empty<ISkillDay>());

			Target.MakeSureExists(new DateOnlyPeriod(), false);
			var context = ResourceCalculationContext.Fetch();
			Target.MakeSureExists(new DateOnlyPeriod(), true);

			ResourceCalculationContext.Fetch()
				.Should().Not.Be.SameInstanceAs(context);
		}

		[Test]
		public void ShouldHandleScheduleChangesInOtherThread_ReuseSameContext()
		{
			var scenario = new Scenario("_");
			var date = new DateOnly(2000, 1, 2);
			var activity = new Activity("_");
			var skill = new Skill("_", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony))
			{
				Activity = activity,
				TimeZone = TimeZoneInfo.Utc
			};
			WorkloadFactory.CreateWorkloadWithFullOpenHours(skill);
			var skillDay = skill.CreateSkillDayWithDemand(scenario, date, 1);
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc);
			agent.AddPeriodWithSkill(new PersonPeriod(date, new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }), skill);
			agent.AddSchedulePeriod(new SchedulePeriod(date, SchedulePeriodType.Day, 1));
			var ass = new PersonAssignment(agent, scenario, date);
			ass.AddActivity(activity, new TimePeriod(0, 0, 1, 0));
			var stateHolder = SchedulerStateHolder.Fill(scenario, date.ToDateOnlyPeriod(), new[] { agent }, new[] { ass }, skillDay);

			Target.MakeSureExists(new DateOnlyPeriod(), true);
			ResourceOptimizationHelperExtended.ResourceCalculateAllDays(new NoSchedulingProgress(), false);
			skillDay.SkillStaffPeriodCollection.First().CalculatedResource.Should().Be.EqualTo(1);


			Task.Factory.StartNew(() =>
			{
				var part = stateHolder.SchedulingResultState.Schedules[agent].ScheduledDay(date);
				part.DeleteMainShift();
				stateHolder.Schedules.Modify(part, ScheduleDayChangeCallback);
			}).Wait();


			Target.MakeSureExists(new DateOnlyPeriod(), false);
			ResourceOptimizationHelperExtended.ResourceCalculateAllDays(new NoSchedulingProgress(), false);
			skillDay.SkillStaffPeriodCollection.First().CalculatedResource.Should().Be.EqualTo(0);
		}

		[TearDown]
		public void Teardown()
		{
			new ResourceCalculationContext(null);
		}
	}
}