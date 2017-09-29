using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.Scheduling
{
	[DomainTest]
	[UseIocForFatClient]
	public class SchedulingDesktopTest : SchedulingScenario
	{
		public DesktopScheduling Target;
		public Func<ISchedulerStateHolder> SchedulerStateHolderFrom;
		public Func<IGridlockManager> LockManager;

		[Test]
		public void ShouldNotScheduleHourlyEmployeesWhenSchedulingFixedStaff()
		{
			var firstDay = new DateOnly(2017, 5, 15);
			var period = new DateOnlyPeriod(firstDay, firstDay);
			var activity = new Activity("_").WithId();
			var skill = new Skill("A").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
			var scenario = new Scenario("_");
			var workTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(0), TimeSpan.FromHours(90), TimeSpan.FromHours(8), TimeSpan.FromHours(0));
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), new ShiftCategory("_").WithId()));
			var contract = new Contract("_") { WorkTimeDirective = workTimeDirective, EmploymentType = EmploymentType.FixedStaffNormalWorkTime };
			var contractHourly = new Contract("_") { WorkTimeDirective = workTimeDirective, EmploymentType = EmploymentType.HourlyStaff };
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet, contract, skill).WithSchedulePeriodOneWeek(firstDay);
			var agentHourly = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet, contractHourly, skill).WithSchedulePeriodOneWeek(firstDay);
			var skillDays = skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay, 10, 10, 10, 10, 10, 10, 10);
			var schedulerStateHolder = SchedulerStateHolderFrom.Fill(scenario, period, new[] { agent, agentHourly }, Enumerable.Empty<IPersonAssignment>(), skillDays);
			var schedulingOptions = new SchedulingOptions { ScheduleEmploymentType = ScheduleEmploymentType.FixedStaff };

			Target.Execute(new NoSchedulingCallback(), schedulingOptions, new NoSchedulingProgress(), new[] { agent, agentHourly }, period);

			schedulerStateHolder.Schedules[agent].ScheduledDay(firstDay).IsScheduled().Should().Be.True();
			schedulerStateHolder.Schedules[agentHourly].ScheduledDay(firstDay).IsScheduled().Should().Be.False();
		}

		[Test]
		public void ShouldNotTouchLockedDays()
		{
			var firstDay = new DateOnly(2017, 5, 15);
			var period = new DateOnlyPeriod(firstDay, firstDay);
			var activity = new Activity("_").WithId();
			var skill = new Skill("A").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
			var scenario = new Scenario("_");
			var workTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(0), TimeSpan.FromHours(90), TimeSpan.FromHours(8), TimeSpan.FromHours(0));
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), new ShiftCategory("_").WithId()));
			var contract = new Contract("_") { WorkTimeDirective = workTimeDirective, EmploymentType = EmploymentType.FixedStaffNormalWorkTime };
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet, contract, skill).WithSchedulePeriodOneWeek(firstDay);
			var agentLocked = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet, contract, skill).WithSchedulePeriodOneWeek(firstDay);
			var skillDays = skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay, 10, 10, 10, 10, 10, 10, 10);
			var schedulerStateHolder = SchedulerStateHolderFrom.Fill(scenario, period, new[] { agentLocked, agent }, Enumerable.Empty<IPersonAssignment>(), skillDays);
			var schedulingOptions = new SchedulingOptions { ScheduleEmploymentType = ScheduleEmploymentType.FixedStaff };
			LockManager().AddLock(agentLocked,firstDay,LockType.Normal);

			Target.Execute(new NoSchedulingCallback(), schedulingOptions, new NoSchedulingProgress(), new[] { agentLocked, agent }, period);

			schedulerStateHolder.Schedules[agentLocked].ScheduledDay(firstDay).IsScheduled().Should().Be.False();
			schedulerStateHolder.Schedules[agent].ScheduledDay(firstDay).IsScheduled().Should().Be.True();
		}

		[Test]
		public void ShouldConsiderAgentsNotPartOfAllPermittedPersons()
		{
			var scenario = new Scenario("_");
			var activity = new Activity("_").WithId();
			var dateOnly = new DateOnly(2010, 1, 1);
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(7, 0, 8, 0, 60), new TimePeriodWithSegment(15, 0, 16, 0, 60), new ShiftCategory("_").WithId()));
			var contract = new ContractWithMaximumTolerance();
			var skill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
			var skillDay = skill.CreateSkillDayWithDemand(scenario, dateOnly, 10);
			var asses = new List<IPersonAssignment>();
			for (var i = 0; i < 10; i++)
			{
				var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet, contract, skill).WithSchedulePeriodOneDay(dateOnly);
				asses.Add(i < 5
					? new PersonAssignment(agent, scenario, dateOnly).ShiftCategory(new ShiftCategory("_").WithId()).WithLayer(activity, new TimePeriod(8, 16))
					: new PersonAssignment(agent, scenario, dateOnly));
			}
			var schedulerStateHolderFrom = SchedulerStateHolderFrom.Fill(scenario, dateOnly.ToDateOnlyPeriod(), asses.Select(x => x.Person), asses, skillDay);
			var unscheduledAgents = asses.Where(x => !x.ShiftLayers.Any()).Select(x => x.Person);
			asses.Select(x => x.Person).Where(x => !unscheduledAgents.Contains(x)).ForEach(x => schedulerStateHolderFrom.AllPermittedPersons.Remove(x));

			Target.Execute(new NoSchedulingCallback(), new SchedulingOptions(), new NoSchedulingProgress(), unscheduledAgents, dateOnly.ToDateOnlyPeriod());

			var schedules = schedulerStateHolderFrom.Schedules.SchedulesForDay(dateOnly);
			schedules.Count(x => x.PersonAssignment().Period.StartDateTime.Hour == 7).Should().Be.EqualTo(5);
			schedules.Count(x => x.PersonAssignment().Period.StartDateTime.Hour == 8).Should().Be.EqualTo(5);
		}

		[Test]
		//[Ignore("#45997 to be fixed")]
		public void ShouldPlaceContractDayOffsCorrect()
		{
			var period = new DateOnlyPeriod(new DateOnly(2017, 8, 27), new DateOnly(2017, 9, 9));
			var activity = new Activity("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 60), new TimePeriodWithSegment(16, 0, 16, 0, 60), new ShiftCategory("_").WithId()));
			var contract = new ContractWithMaximumTolerance();
			var contractSchedule = new ContractSchedule("_");
			var contractScheduleWeek1 = new ContractScheduleWeek();
			var contractScheduleWeek2 = new ContractScheduleWeek();
			contractScheduleWeek1.Add(DayOfWeek.Monday, true);
			contractScheduleWeek1.Add(DayOfWeek.Tuesday, true);
			contractScheduleWeek1.Add(DayOfWeek.Wednesday, true);
			contractScheduleWeek1.Add(DayOfWeek.Thursday, true);
			contractScheduleWeek1.Add(DayOfWeek.Friday, true);
			contractScheduleWeek1.Add(DayOfWeek.Saturday, true);
			contractScheduleWeek1.Add(DayOfWeek.Sunday, false);	//<-
			contractScheduleWeek2.Add(DayOfWeek.Monday, true);
			contractScheduleWeek2.Add(DayOfWeek.Tuesday, false); //<-
			contractScheduleWeek2.Add(DayOfWeek.Wednesday, true);
			contractScheduleWeek2.Add(DayOfWeek.Thursday, true);
			contractScheduleWeek2.Add(DayOfWeek.Friday, true);
			contractScheduleWeek2.Add(DayOfWeek.Saturday, true);
			contractScheduleWeek2.Add(DayOfWeek.Sunday, true);
			contractSchedule.AddContractScheduleWeek(contractScheduleWeek1);
			contractSchedule.AddContractScheduleWeek(contractScheduleWeek2);	
			var skill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
			var agent = new Person().WithId().InTimeZone(TimeZoneInfoFactory.StockholmTimeZoneInfo()).WithPersonPeriod(ruleSet, contract, skill).WithSchedulePeriodTwoWeeks(period.StartDate);
			agent.FirstDayOfWeek = DayOfWeek.Sunday;
			agent.Period(period.StartDate).PersonContract.ContractSchedule = contractSchedule;
			var schedulerStateHolder = SchedulerStateHolderFrom.Fill(new Scenario("_"), period, new[] {agent }, Enumerable.Empty<IPersonAssignment>(), Enumerable.Empty<ISkillDay>());
			
			Target.Execute(new NoSchedulingCallback(), new SchedulingOptions(), new NoSchedulingProgress(), new []{agent}, period);

			schedulerStateHolder.Schedules[agent].ScheduledDay(period.StartDate).PersonAssignment(true).DayOff().Should().Not.Be.Null();
			schedulerStateHolder.Schedules[agent].ScheduledDay(period.StartDate.AddDays(9)).PersonAssignment(true).DayOff().Should().Not.Be.Null();
		}

		public SchedulingDesktopTest(bool resourcePlannerMergeTeamblockClassicScheduling44289) : base(resourcePlannerMergeTeamblockClassicScheduling44289)
		{
		}
	}
}
