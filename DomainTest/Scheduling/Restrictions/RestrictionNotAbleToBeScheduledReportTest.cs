using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.DomainTest.SchedulingScenarios;
using Teleopti.Ccc.DomainTest.SchedulingScenarios.Scheduling;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Scheduling;


namespace Teleopti.Ccc.DomainTest.Scheduling.Restrictions
{
	[DomainTest]
	[UseIocForFatClient]
	public class RestrictionNotAbleToBeScheduledReportTest : SchedulingScenario
	{
		public RestrictionNotAbleToBeScheduledReport Target;
		public Func<ISchedulerStateHolder> SchedulerStateHolderFrom;

		[Test]
		public void ShouldOnlyReportOnAllAgents()
		{
			var period = new DateOnlyPeriod(2017, 12, 01, 2017, 12, 31);
			var activity = new Activity().WithId();
			var skill = new Skill().For(activity).DefaultResolution(60).WithId().IsOpen();
			var scenario = new Scenario();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity,
				new TimePeriodWithSegment(8, 0, 10, 0, 60), new TimePeriodWithSegment(16, 0, 18, 0, 60),
				new ShiftCategory("_").WithId()));
			var agent1 = new Person().WithId()
				.WithPersonPeriod(new RuleSetBag(ruleSet), skill)
				.WithSchedulePeriodOneMonth(period.StartDate);
			agent1.Period(period.StartDate).PersonContract = new PersonContract(new Contract("_"), new PartTimePercentage("_"),
				new ContractScheduleWorkingMondayToFriday());
			var agent2 = new Person().WithId()
				.WithPersonPeriod(new RuleSetBag(ruleSet), skill)
				.WithSchedulePeriodOneMonth(period.StartDate);
			agent2.Period(period.StartDate).PersonContract = new PersonContract(new Contract("_"), new PartTimePercentage("_"),
				new ContractScheduleWorkingMondayToFriday());
			var skillDays = skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, period, 1);
			var preferenceDays = new List<IPreferenceDay>();
			foreach (var dateOnly in period.DayCollection())
			{
				if (dateOnly.DayOfWeek == DayOfWeek.Saturday || dateOnly.DayOfWeek == DayOfWeek.Sunday)
					continue;

				preferenceDays.Add(new PreferenceDay(agent1, dateOnly,
					new PreferenceRestriction { WorkTimeLimitation = new WorkTimeLimitation(TimeSpan.FromHours(8), TimeSpan.FromHours(8)) }));

				preferenceDays.Add(new PreferenceDay(agent2, dateOnly,
					new PreferenceRestriction { StartTimeLimitation = new StartTimeLimitation(TimeSpan.FromHours(11), null) }));
			}

			SchedulerStateHolderFrom.Fill(scenario, period, new[] { agent1, agent2 }, preferenceDays, skillDays);

			var result = Target.Create(period, new[] {agent1, agent2}, new NoSchedulingProgress());

			result.Count().Should().Be.EqualTo(2);
			result.Last().Agent.Should().Be.EqualTo(agent2);
			result.First().Agent.Should().Be.EqualTo(agent1);
			result.First().Reason.Should().Be.EqualTo(RestrictionNotAbleToBeScheduledReason.NoIssue);

		}

		[Test]
		public void ShouldReportFirstFoundIssue()
		{
			var period = new DateOnlyPeriod(2017, 12, 01, 2017, 12, 31);
			var activity = new Activity().WithId();
			var skill = new Skill().For(activity).DefaultResolution(60).WithId().IsOpen();
			var scenario = new Scenario();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity,
				new TimePeriodWithSegment(8, 0, 10, 0, 60), new TimePeriodWithSegment(16, 0, 18, 0, 60),
				new ShiftCategory("_").WithId()));
			var agent1 = new Person().WithId()
				.WithPersonPeriod(new RuleSetBag(ruleSet), skill)
				.WithSchedulePeriodOneMonth(period.StartDate);
			agent1.Period(period.StartDate).PersonContract = new PersonContract(new Contract("_"), new PartTimePercentage("_"),
				new ContractScheduleWorkingMondayToFriday());
			
			var skillDays = skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, period, 1);
			var preferenceDays = new List<IPreferenceDay>();
			foreach (var dateOnly in period.DayCollection())
			{
				preferenceDays.Add(new PreferenceDay(agent1, dateOnly,
					new PreferenceRestriction { StartTimeLimitation = new StartTimeLimitation(TimeSpan.FromHours(8), null) }));
			}

			SchedulerStateHolderFrom.Fill(scenario, period, new[] { agent1 }, preferenceDays, skillDays);

			var result = Target.Create(period, new[] { agent1 }, new NoSchedulingProgress());

			result.Count().Should().Be.EqualTo(1);
			result.First().Reason.Should().Be.EqualTo(RestrictionNotAbleToBeScheduledReason.TooMuchWorkTimeInPeriod);
			result.First().Period.Should().Be.EqualTo(period);
		}

		[Test]
		public void ShouldNotLeaveAddedDaysOffWhenDone()
		{
			var period = new DateOnlyPeriod(2017, 12, 01, 2017, 12, 31);
			var activity = new Activity().WithId();
			var skill = new Skill().For(activity).DefaultResolution(60).WithId().IsOpen();
			var scenario = new Scenario();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity,
				new TimePeriodWithSegment(8, 0, 10, 0, 60), new TimePeriodWithSegment(16, 0, 18, 0, 60),
				new ShiftCategory("_").WithId()));
			var agent1 = new Person().WithId()
				.WithPersonPeriod(new RuleSetBag(ruleSet), skill)
				.WithSchedulePeriodOneMonth(period.StartDate);
			agent1.Period(period.StartDate).PersonContract = new PersonContract(new Contract("_"), new PartTimePercentage("_"),
				new ContractScheduleWorkingMondayToFriday());

			var skillDays = skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, period, 1);

			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, period, new[] { agent1 }, new List<IPreferenceDay>(), skillDays);
			

			var result = Target.Create(period, new[] { agent1 }, new NoSchedulingProgress());

			result.Count().Should().Be.EqualTo(1);
			foreach (var dateOnly in period.DayCollection())
			{
				stateHolder.Schedules[agent1].ScheduledDay(dateOnly).PersonAssignment(true).DayOff().Should().Be.Null();
			}
		}

		[Test]
		public void ShouldHandleAgentWithPersonPeriodStartingLaterThanSchedulePeriod()
		{
			var period = new DateOnlyPeriod(2017, 12, 01, 2017, 12, 31);
			var activity = new Activity().WithId();
			var skill = new Skill().For(activity).DefaultResolution(60).WithId().IsOpen();
			var scenario = new Scenario();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity,
				new TimePeriodWithSegment(8, 0, 10, 0, 60), new TimePeriodWithSegment(16, 0, 18, 0, 60),
				new ShiftCategory("_").WithId()));
			var agent1 = new Person().WithId()
				.WithPersonPeriod(period.StartDate.AddDays(3), new RuleSetBag(ruleSet), skill)
				.WithSchedulePeriodOneMonth(period.StartDate);
			agent1.Period(period.StartDate.AddDays(3)).PersonContract = new PersonContract(new Contract("_"), new PartTimePercentage("_"),
				new ContractScheduleWorkingMondayToFriday());

			var skillDays = skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, period, 1);

			SchedulerStateHolderFrom.Fill(scenario, period, new[] { agent1 }, new List<IPreferenceDay>(), skillDays);

			var result = Target.Create(period, new[] { agent1 }, new NoSchedulingProgress());

			result.Count().Should().Be.EqualTo(1);
			result.First().Reason.Should().Be.EqualTo(RestrictionNotAbleToBeScheduledReason.NoRestrictions);
		}

		[Test]
		public void ShouldHandleAgentWithPersonPeriodStartingLaterThanSchedulePeriodAndIssuesInWeekNumberTwo()
		{
			var period = new DateOnlyPeriod(2017, 12, 01, 2017, 12, 31);
			var activity = new Activity().WithId();
			var skill = new Skill().For(activity).DefaultResolution(60).WithId().IsOpen();
			var scenario = new Scenario();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity,
				new TimePeriodWithSegment(8, 0, 10, 0, 60), new TimePeriodWithSegment(16, 0, 18, 0, 60),
				new ShiftCategory("_").WithId()));
			var agent1 = new Person().WithId()
				.WithPersonPeriod(period.StartDate.AddDays(3), new RuleSetBag(ruleSet), skill)
				.WithSchedulePeriodOneMonth(period.StartDate);
			agent1.Period(period.StartDate.AddDays(3)).PersonContract = new PersonContract(new Contract("_"), new PartTimePercentage("_"),
				new ContractScheduleWorkingMondayToFriday());

			var skillDays = skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, period, 1);
			var preferenceDays = new List<IPreferenceDay>();
			for (int i = 0; i < 7; i++)
			{
				var mondaySecondWeek = new DateOnly(2017,12,4);
				preferenceDays.Add(new PreferenceDay(agent1, mondaySecondWeek.AddDays(i),
					new PreferenceRestriction { WorkTimeLimitation = new WorkTimeLimitation(TimeSpan.FromHours(8), TimeSpan.FromHours(8)) }));
			}
			
			SchedulerStateHolderFrom.Fill(scenario, period, new[] { agent1 }, preferenceDays, skillDays);

			var result = Target.Create(period, new[] { agent1 }, new NoSchedulingProgress());

			result.Count().Should().Be.EqualTo(1);
			result.First().Reason.Should().Be.EqualTo(RestrictionNotAbleToBeScheduledReason.TooMuchWorkTimeInPeriod);
		}

		[Test]
		public void ShouldHandleMultipleMatrixesInSelection()
		{
			var period = new DateOnlyPeriod(2018, 4, 2, 2018, 4, 29);
			var activity = new Activity().WithId();
			var skill = new Skill().For(activity).DefaultResolution(60).WithId().IsOpen();
			var scenario = new Scenario();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity,
				new TimePeriodWithSegment(8, 0, 10, 0, 60), new TimePeriodWithSegment(16, 0, 18, 0, 60),
				new ShiftCategory("_").WithId()));
			var agent1 = new Person().WithId()
				.WithPersonPeriod(period.StartDate.AddDays(3), new RuleSetBag(ruleSet), skill)
				.WithSchedulePeriodTwoWeeks(period.StartDate);
			agent1.Period(period.StartDate.AddDays(3)).PersonContract = new PersonContract(new Contract("_"), new PartTimePercentage("_"),
				new ContractScheduleWorkingMondayToFriday());

			var skillDays = skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, period, 1);
			var preferenceDays = new List<IPreferenceDay>();
			for (int i = 0; i < 7; i++)
			{
				var mondaySecondWeekFirstSchedulePeriod = new DateOnly(2018, 4, 9);
				preferenceDays.Add(new PreferenceDay(agent1, mondaySecondWeekFirstSchedulePeriod.AddDays(i),
					new PreferenceRestriction { WorkTimeLimitation = new WorkTimeLimitation(TimeSpan.FromHours(8), TimeSpan.FromHours(8)) }));

				var mondaySecondWeekSecondSchedulePeriod = new DateOnly(2018, 4, 23);
				preferenceDays.Add(new PreferenceDay(agent1, mondaySecondWeekSecondSchedulePeriod.AddDays(i),
					new PreferenceRestriction { WorkTimeLimitation = new WorkTimeLimitation(TimeSpan.FromHours(8), TimeSpan.FromHours(8)) }));
			}

			SchedulerStateHolderFrom.Fill(scenario, period, new[] { agent1 }, preferenceDays, skillDays);

			var result = Target.Create(period, new[] { agent1 }, new NoSchedulingProgress()).ToList();

			result.Count.Should().Be.EqualTo(2);
			result[0].Reason.Should().Be.EqualTo(RestrictionNotAbleToBeScheduledReason.TooMuchWorkTimeInPeriod);
			result[0].Period.Should().Be.EqualTo(new DateOnlyPeriod(new DateOnly(2018, 4, 9), new DateOnly(2018, 4, 9).AddDays(6)));
			result[1].Reason.Should().Be.EqualTo(RestrictionNotAbleToBeScheduledReason.TooMuchWorkTimeInPeriod);
			result[1].Period.Should().Be.EqualTo(new DateOnlyPeriod(new DateOnly(2018, 4, 23), new DateOnly(2018, 4, 23).AddDays(6)));
		}

		public RestrictionNotAbleToBeScheduledReportTest(ResourcePlannerTestParameters resourcePlannerTestParameters) : base(resourcePlannerTestParameters)
		{
		}
	}
}