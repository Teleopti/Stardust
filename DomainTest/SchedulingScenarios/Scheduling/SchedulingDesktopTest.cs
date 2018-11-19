using System;
using System.Collections.Generic;
using System.Linq;
using NPOI.SS.Formula.Functions;
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
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Scheduling;
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
		public void ShouldPutDayOffOnDayWithPersonalActivityAfterMidnight()
		{
			var date = new DateOnly(2018, 9, 10);
			var activity = new Activity().WithId();
			var skill = new Skill().For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
			var scenario = new Scenario();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 60), new TimePeriodWithSegment(16, 0, 16, 0, 60), new ShiftCategory("_").WithId()));
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet,new ContractScheduleWorkingMondayToFriday(), skill).WithSchedulePeriodOneWeek(date);
			var skillDays = skill.CreateSkillDayWithDemand(scenario, DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1), 1);
			var dayToSchedule = date.AddDays(5);
			var asses = new List<IPersonAssignment>();
			for (var i = 0; i < 7; i++)
			{
				asses.Add(new PersonAssignment(agent, scenario, date.AddDays(i)).WithId());
			}
			asses.Single(x => x.Date == dayToSchedule)
				.WithPersonalLayer(activity, new TimePeriod(30, 31));
			var schedulerStateHolder = SchedulerStateHolderFrom.Fill(scenario, DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1), agent, asses, skillDays);
			
			Target.Execute(new NoSchedulingCallback(), new SchedulingOptions(), new NoSchedulingProgress(), new[]{agent}, dayToSchedule.ToDateOnlyPeriod());

			schedulerStateHolder.Schedules[agent].ScheduledDay(dayToSchedule).PersonAssignment(true).DayOff()
				.Should().Not.Be.Null();
		}
		
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
			asses.Select(x => x.Person).Where(x => !unscheduledAgents.Contains(x)).ForEach(x => schedulerStateHolderFrom.ChoosenAgents.Remove(x));

			Target.Execute(new NoSchedulingCallback(), new SchedulingOptions(), new NoSchedulingProgress(), unscheduledAgents, dateOnly.ToDateOnlyPeriod());

			var schedules = schedulerStateHolderFrom.Schedules.SchedulesForDay(dateOnly);
			schedules.Count(x => x.PersonAssignment().Period.StartDateTime.Hour == 7).Should().Be.EqualTo(5);
			schedules.Count(x => x.PersonAssignment().Period.StartDateTime.Hour == 8).Should().Be.EqualTo(5);
		}

		[Test]
		public void ShouldPlaceContractDayOffsCorrect()
		{
			var period = new DateOnlyPeriod(new DateOnly(2017, 8, 27), new DateOnly(2017, 9, 9));
			var activity = new Activity("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 60), new TimePeriodWithSegment(16, 0, 16, 0, 60), new ShiftCategory("_").WithId()));
			var contract = new ContractWithMaximumTolerance();
			var contractSchedule = new ContractSchedule("_");
			var contractScheduleWeek1 = new ContractScheduleWeek();
			contractScheduleWeek1.SetWorkdaysExcept(DayOfWeek.Sunday);
			var contractScheduleWeek2 = new ContractScheduleWeek();
			contractScheduleWeek2.SetWorkdaysExcept(DayOfWeek.Tuesday);
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

		[Test]
		public void ShouldNotCrashWhenSolvingWeeklyRest()
		{
			var firstDay = new DateOnly(2017, 5, 14);
			var firstDayPreviousWeek = firstDay.AddDays(-6);
			var dataPeriod = new DateOnlyPeriod(firstDayPreviousWeek, firstDay.AddDays(7));
			var schedulePeriod = new DateOnlyPeriod(firstDay, firstDay.AddDays(7));
			var shiftCategory = new ShiftCategory("_").WithId();
			var activity = new Activity("_").WithId();
			activity.InWorkTime = true;
			var skill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
			var scenario = new Scenario("_");
			var weeklyRestToLongToBeFulfilled = TimeSpan.FromHours(48);
			var workTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(0), TimeSpan.FromHours(90), TimeSpan.FromHours(8), weeklyRestToLongToBeFulfilled);
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), new ShiftCategory("_").WithId()));
			var contract = new Contract("_") { WorkTimeDirective = workTimeDirective, EmploymentType = EmploymentType.FixedStaffNormalWorkTime };
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet, contract, skill).WithSchedulePeriodOneWeek(firstDay);
			var skillDays = skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay, 10, 10, 10, 10, 10, 10, 10);	
			var asses = new List<IPersonAssignment>();
			for (var i = 0; i < 8; i++)
			{
				var ass = new PersonAssignment(agent, scenario, firstDayPreviousWeek.AddDays(i)).ShiftCategory(shiftCategory).WithLayer(activity, new TimePeriod(8, 16));
				asses.Add(ass);
				if (i==6) ass.SetDayOff(new DayOffTemplate());		
			}
			SchedulerStateHolderFrom.Fill(scenario, dataPeriod, new[] { agent }, asses, skillDays);
			var schedulingOptions = new SchedulingOptions { ScheduleEmploymentType = ScheduleEmploymentType.FixedStaff };

			Assert.DoesNotThrow(() =>
			{
				Target.Execute(new NoSchedulingCallback(), schedulingOptions, new NoSchedulingProgress(), new[] { agent }, schedulePeriod);
			});
		}

		[Test]
		public void ShouldConsiderPersonPeriodsInsideSelectedPeriod()
		{
			var firstDay = new DateOnly(2017, 5, 15);
			var period = DateOnlyPeriod.CreateWithNumberOfWeeks(firstDay, 1);
			var personPeriodFirstDay = firstDay.AddDays(1);
			var activity = new Activity().WithId();
			var skill = new Skill().For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
			var scenario = new Scenario();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), new ShiftCategory("_").WithId()));
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc)
				.WithSchedulePeriodOneWeek(personPeriodFirstDay)
				.WithPersonPeriod(personPeriodFirstDay, ruleSet, skill);
			var skillDays = skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay, 10, 10, 10, 10, 10, 10, 10);
			var schedulerStateHolder = SchedulerStateHolderFrom.Fill(scenario, period, new[] { agent }, Enumerable.Empty<IPersonAssignment>(), skillDays);
			var schedulingOptions = new SchedulingOptions { ScheduleEmploymentType = ScheduleEmploymentType.FixedStaff };

			Target.Execute(new NoSchedulingCallback(), schedulingOptions, new NoSchedulingProgress(), new[] { agent }, period);
			
			for (var i = 0; i < 6; i++)
			{
				if (i == 0)
				{
					schedulerStateHolder.Schedules[agent].ScheduledDay(firstDay.AddDays(i)).IsScheduled().Should().Be.False();
				}
				else
				{
					schedulerStateHolder.Schedules[agent].ScheduledDay(firstDay.AddDays(i)).IsScheduled().Should().Be.True();
				}
			}
		}

		[Test]
		public void ShouldValidateSolvedWeeklyRest()
		{
			var date = new DateOnly(2017, 5, 15);
			var schedulePeriod = DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1);
			var shiftCategory = new ShiftCategory("_").WithId();
			var activity = new Activity{InWorkTime = true}.WithId();
			var skill = new Skill().For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
			var scenario = new Scenario();
			var weeklyRest = TimeSpan.FromHours(20);
			var workTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(0), TimeSpan.FromHours(90), TimeSpan.FromHours(8), weeklyRest);
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), new ShiftCategory("_").WithId()));
			var contract = new Contract("_") { WorkTimeDirective = workTimeDirective, EmploymentType = EmploymentType.FixedStaffNormalWorkTime };
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet, contract, skill).WithSchedulePeriodOneWeek(date);
			var skillDays = skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, date, 10, 10, 10, 10, 10, 10, 10);
			var asses = new List<IPersonAssignment>();
			for (var i = -1; i < 8; i++)
			{
				var ass = new PersonAssignment(agent, scenario, date.AddDays(i)).ShiftCategory(shiftCategory).WithLayer(activity, new TimePeriod(8, 16));
				asses.Add(ass);
			}
			var schedulerStateHolder = SchedulerStateHolderFrom.Fill(scenario, new DateOnlyPeriod(date.AddDays(-1), date.AddDays(7)), agent, asses, skillDays);
			schedulerStateHolder.Schedules.ValidateBusinessRulesOnPersons(new []{agent}, NewBusinessRuleCollection.All(schedulerStateHolder.SchedulingResultState));

			Target.Execute(new NoSchedulingCallback(), new SchedulingOptions(), new NoSchedulingProgress(), new[] { agent }, schedulePeriod);

			schedulerStateHolder.Schedules[agent].BusinessRuleResponseInternalCollection.Should().Be.Empty();
		}

		[Test]
		public void ShouldChooseShiftWithCorrectLengthWhenDifferentOpenHoursOnDays()
		{
			var date = new DateOnly(2017, 5, 15);
			var period = new DateOnlyPeriod(date.AddDays(6), date.AddDays(6));
			var activity = new Activity {InContractTime = true}.WithId();
			var days = new Dictionary<DayOfWeek, TimePeriod>
			{
				{DayOfWeek.Saturday, new TimePeriod(6, 22)},
				{DayOfWeek.Sunday, new TimePeriod(7, 22)}
			};
			var skill = new Skill().For(activity).WithId().InTimeZone(TimeZoneInfo.Utc).IsOpen(days);
			var scenario = new Scenario();
			var shiftCategory = new ShiftCategory().WithId();
			var workTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(0), TimeSpan.FromHours(90), TimeSpan.FromHours(0), TimeSpan.FromHours(0));
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 15, 15), new ShiftCategory("_").WithId()));
			var contract = new Contract("_") { WorkTimeDirective = workTimeDirective, EmploymentType = EmploymentType.FixedStaffNormalWorkTime, WorkTime = new WorkTime(TimeSpan.FromHours(8)) };
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet, contract, skill).WithSchedulePeriodOneWeek(date);
			var skillDays = skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, date, 10, 10, 10, 10, 10, 10, 10);
			var asses = new List<IPersonAssignment>();
			for (var i = 0; i < 6; i++)
			{
				asses.Add(new PersonAssignment(agent, scenario, date.AddDays(i)).ShiftCategory(shiftCategory).WithLayer(activity, new TimePeriod(8, 16)));
			}
			var schedulerStateHolder = SchedulerStateHolderFrom.Fill(scenario, period, new[] { agent }, asses, skillDays);
			var schedulingOptions = new SchedulingOptions { ScheduleEmploymentType = ScheduleEmploymentType.FixedStaff };

			Target.Execute(new NoSchedulingCallback(), schedulingOptions, new NoSchedulingProgress(), new[] { agent }, period);

			schedulerStateHolder.Schedules[agent].ScheduledDay(date.AddDays(6)).IsScheduled().Should().Be.True();
		}

		[Test]
		public void ShouldNotPlaceMissingDayOffsOnPartlySelectedSchedulePeriod()
		{
			var activity = new Activity().WithId();
			var period = new DateOnlyPeriod(new DateOnly(2018, 2, 1), new DateOnly(2018, 3, 4));
			var skill = new Skill().For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 15, 15), new ShiftCategory("_").WithId()));
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet, new ContractWithMaximumTolerance(), skill).WithSchedulePeriodOneMonth(new DateOnly(2018, 1, 1));
			var contractSchedule = new ContractSchedule("_");
			var contractScheduleWeek = new ContractScheduleWeek();
			contractScheduleWeek.SetWorkdaysExcept(DayOfWeek.Saturday, DayOfWeek.Sunday);
			contractSchedule.AddContractScheduleWeek(contractScheduleWeek);
			agent.Period(period.StartDate).PersonContract.ContractSchedule = contractSchedule;
			var schedulerStateHolder = SchedulerStateHolderFrom.Fill(new Scenario(), period, new[] { agent }, Enumerable.Empty<IPersonAssignment>(), Enumerable.Empty<ISkillDay>());

			Target.Execute(new NoSchedulingCallback(), new SchedulingOptions(), new NoSchedulingProgress(), new[] { agent }, period);

			schedulerStateHolder.Schedules[agent].ScheduledDayCollection(new DateOnlyPeriod(2018, 2, 1, 2018, 2, 28))
				.Count(x => x.HasDayOff()).Should().Be.EqualTo(8);
			schedulerStateHolder.Schedules[agent].ScheduledDayCollection(new DateOnlyPeriod(2018, 3, 1, 2018, 3, 4))
				.Count(x => x.HasDayOff()).Should().Be.EqualTo(2);
		}

		[TestCase(12, ExpectedResult = true)]
		[TestCase(6, ExpectedResult = false)]
		public bool ShouldScheduleWhereShiftDoNotInterfereWithDayOff(int anchor)
		{
			var date = new DateOnly(2018, 8, 27);
			var activity = new Activity();
			var skill = new Skill().For(activity).WithId().IsOpen();
			var scenario = new Scenario();
			var shiftCategory = new ShiftCategory().WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(16, 0, 16, 0, 15), new TimePeriodWithSegment(24, 0, 24, 0, 15), shiftCategory));
			var agent = new Person().WithId().WithPersonPeriod(ruleSet, skill).WithSchedulePeriodOneWeek(date);
			agent.SchedulePeriod(date).SetDaysOff(1);
			var skillDays = skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, date, 1, 1, 1, 1, 1, 1, 1);
			var dayOff = new DayOffTemplate { Anchor = TimeSpan.FromHours(anchor) };
			dayOff.SetTargetAndFlexibility(TimeSpan.FromHours(24), TimeSpan.Zero);
			var ass = new PersonAssignment(agent, scenario, date.AddDays(6)).WithDayOff(dayOff);
			var schedulerStateHolder = SchedulerStateHolderFrom.Fill(scenario, new DateOnlyPeriod(date, date.AddDays(7)), new[] { agent }, new[] { ass }, skillDays);
			var schedulingOptions = new SchedulingOptions { ScheduleEmploymentType = ScheduleEmploymentType.FixedStaff };

			Target.Execute(new NoSchedulingCallback(), schedulingOptions, new NoSchedulingProgress(), new[] { agent }, new DateOnlyPeriod(date.AddDays(5), date.AddDays(5)));

			return schedulerStateHolder.Schedules[agent].ScheduledDay(date.AddDays(5)).IsScheduled();
		}

		[Test]
		[Ignore("76118 to be fixed")]
		public void ShouldConsiderOpenHoursWhenUsingAverageShiftLength([Values(18, 12)] int endHour )
		{
			var date = new DateOnly(2018, 10, 1);
			var period = DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1);
			var activity = new Activity().WithId();
			var scenario = new Scenario();
			var days = new Dictionary<DayOfWeek, TimePeriod>
			{
				{DayOfWeek.Wednesday, new TimePeriod(8, 18)},
				{DayOfWeek.Thursday, new TimePeriod(8, 18)},
				{DayOfWeek.Friday, new TimePeriod(8, 18)},
				{DayOfWeek.Saturday, new TimePeriod(8, 18)},
				{DayOfWeek.Sunday, new TimePeriod(8, endHour)}

			};
			var skill = new Skill().For(activity).WithId().InTimeZone(TimeZoneInfo.Utc).IsOpen(days);	
			var contract = new Contract("_") { WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(40), TimeSpan.FromHours(40), TimeSpan.FromHours(11), TimeSpan.FromHours(36)) };
			var ruleSet8And9 = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 60), new TimePeriodWithSegment(16, 0, 17, 0, 60), new ShiftCategory().WithId()));
			var ruleSet4 = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 60), new TimePeriodWithSegment(12, 0, 12, 0, 60), new ShiftCategory().WithId()));
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(new RuleSetBag(ruleSet8And9, ruleSet4), contract, skill).WithSchedulePeriodOneWeek(date);
			agent.SchedulePeriod(date).SetDaysOff(2);
			var skillDays = skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, date.AddDays(2), 10, 10, 10, 10, 10);
			var assDayOff1 = new PersonAssignment(agent, scenario, date).WithDayOff(new DayOffTemplate());
			var assDayOff2 = new PersonAssignment(agent, scenario, date.AddDays(1)).WithDayOff(new DayOffTemplate());
			var schedulerStateHolder = SchedulerStateHolderFrom.Fill(scenario, period, new[]{ agent }, new []{ assDayOff1, assDayOff2 }, skillDays);
			
			Target.Execute(new NoSchedulingCallback(), new SchedulingOptions { UseAverageShiftLengths = true }, new NoSchedulingProgress(), new[]{ agent }, period);

			for (var i = 0; i < 7; i++)
			{
				schedulerStateHolder.Schedules[agent].ScheduledDay(date.AddDays(i)).IsScheduled().Should().Be.True();
			}
		}

		public SchedulingDesktopTest(ResourcePlannerTestParameters resourcePlannerTestParameters) : base(resourcePlannerTestParameters)
		{
		}
	}
}
