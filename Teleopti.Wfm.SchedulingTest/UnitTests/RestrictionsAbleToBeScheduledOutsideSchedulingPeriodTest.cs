using System;
using System.Collections.Generic;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Scheduling;
using Teleopti.Wfm.SchedulingTest.SchedulingScenarios;
using Teleopti.Wfm.SchedulingTest.SchedulingScenarios.Scheduling;

namespace Teleopti.Wfm.SchedulingTest.UnitTests
{
	[DomainTest]
	[UseIocForFatClient]
	public class RestrictionsAbleToBeScheduledOutsideSchedulingPeriodTest : SchedulingScenario
	{
		public RestrictionsAbleToBeScheduled Target;
		public DesktopScheduling Target2;
		public Func<ISchedulerStateHolder> SchedulerStateHolderFrom;

		[Test]
		public void ShouldHandleMonthEndingOnThursday()
		{
			var period = createStandardSetup(out var scenario, out var agent, out var skillDays);
			var extendedPeriod = new DateOnlyPeriod(period.StartDate, period.EndDate.AddDays(6));
			var preferenceDays = new List<IPersistableScheduleData>();
			foreach (var dateOnly in period.DayCollection())
			{
				if (dateOnly.DayOfWeek == DayOfWeek.Saturday || dateOnly.DayOfWeek == DayOfWeek.Sunday)
				{
					preferenceDays.Add(new PreferenceDay(agent, dateOnly,
						new PreferenceRestriction { DayOffTemplate = new DayOffTemplate() }));
				}
				else
				{
					preferenceDays.Add(new PreferenceDay(agent, dateOnly,
						new PreferenceRestriction { WorkTimeLimitation = new WorkTimeLimitation(TimeSpan.FromHours(8), TimeSpan.FromHours(8)) }));
				}
			}

			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, period, new[] { agent }, preferenceDays, skillDays);

			var result = Target.Execute(agent.VirtualSchedulePeriod(period.StartDate));
			result.Reason.Should().Be.EqualTo(RestrictionNotAbleToBeScheduledReason.NoIssue);

			Target2.Execute(new NoSchedulingCallback(), new SchedulingOptions(), new NoSchedulingProgress(), new[] { agent }, extendedPeriod);
			stateHolder.Schedules[agent].CalculatedContractTimeHolderOnPeriod(period).TotalHours.Should().Be
				.EqualTo(176);
		}

		[Test]
		public void ShouldCheckOnWeekMaxTime()
		{
			var period = createStandardSetup(out var scenario, out var agent, out var skillDays);
			var extendedPeriod = new DateOnlyPeriod(period.StartDate, period.EndDate.AddDays(6));
			var preferenceDays = new List<IPersistableScheduleData>();

			preferenceDays.Add(new PreferenceDay(agent, new DateOnly(2017, 11, 6), 
				new PreferenceRestriction { WorkTimeLimitation = new WorkTimeLimitation(TimeSpan.FromHours(9), TimeSpan.FromHours(9)) }));
			preferenceDays.Add(new PreferenceDay(agent, new DateOnly(2017, 11, 7),
				new PreferenceRestriction { WorkTimeLimitation = new WorkTimeLimitation(TimeSpan.FromHours(8), TimeSpan.FromHours(8)) }));
			preferenceDays.Add(new PreferenceDay(agent, new DateOnly(2017, 11, 8),
				new PreferenceRestriction { WorkTimeLimitation = new WorkTimeLimitation(TimeSpan.FromHours(8), TimeSpan.FromHours(8)) }));
			preferenceDays.Add(new PreferenceDay(agent, new DateOnly(2017, 11, 9),
				new PreferenceRestriction { WorkTimeLimitation = new WorkTimeLimitation(TimeSpan.FromHours(8), TimeSpan.FromHours(8)) }));
			preferenceDays.Add(new PreferenceDay(agent, new DateOnly(2017, 11, 10),
				new PreferenceRestriction { WorkTimeLimitation = new WorkTimeLimitation(TimeSpan.FromHours(8), TimeSpan.FromHours(8)) }));
			preferenceDays.Add(new PreferenceDay(agent, new DateOnly(2017, 11, 11),
				new PreferenceRestriction { WorkTimeLimitation = new WorkTimeLimitation(TimeSpan.FromHours(8), TimeSpan.FromHours(8)) }));

			preferenceDays.Add(new PreferenceDay(agent, new DateOnly(2017, 11, 12),
				new PreferenceRestriction { DayOffTemplate = new DayOffTemplate() }));

			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, period, new[] { agent }, preferenceDays, skillDays);

			var result = Target.Execute(agent.VirtualSchedulePeriod(period.StartDate));
			result.Reason.Should().Be.EqualTo(RestrictionNotAbleToBeScheduledReason.TooMuchWorkTimeInPeriod);
			result.Period.Should().Be.EqualTo(new DateOnlyPeriod(new DateOnly(2017, 11, 6), new DateOnly(2017, 11, 6).AddDays(6)));

			Target2.Execute(new NoSchedulingCallback(), new SchedulingOptions(), new NoSchedulingProgress(), new[] { agent }, extendedPeriod);
			stateHolder.Schedules[agent].CalculatedContractTimeHolderOnPeriod(period).TotalHours.Should().Be.LessThan(176);
		}

		[Test]
		public void ShouldHandleFirstWeekWhenTheAgentIsOnBoardingThisPeriod()
		{
			var period = createStandardSetup(out var scenario, out var agent, out var skillDays);
			agent.Period(period.StartDate).StartDate = period.StartDate;
			var extendedPeriod = new DateOnlyPeriod(period.StartDate, period.EndDate.AddDays(6));
			var preferenceDaysOrAss = new List<IPersistableScheduleData>();
			var shiftCategory = new ShiftCategory("_");
			var activity = new Activity().WithId();
			var firstWeekStartDate = new DateOnly(2017, 10, 30);
			for (int i = 0; i < 2; i++)
			{
				var ass = new PersonAssignment(agent, scenario, firstWeekStartDate.AddDays(i));
				ass.AddActivity(activity, new TimePeriod(8, 18));
				ass.SetShiftCategory(shiftCategory);
				preferenceDaysOrAss.Add(ass);
			}

			preferenceDaysOrAss.Add(new PreferenceDay(agent, new DateOnly(2017, 11, 1),
				new PreferenceRestriction { WorkTimeLimitation = new WorkTimeLimitation(TimeSpan.FromHours(10), TimeSpan.FromHours(10)) }));
			preferenceDaysOrAss.Add(new PreferenceDay(agent, new DateOnly(2017, 11, 2),
				new PreferenceRestriction { WorkTimeLimitation = new WorkTimeLimitation(TimeSpan.FromHours(10), TimeSpan.FromHours(10)) }));
			preferenceDaysOrAss.Add(new PreferenceDay(agent, new DateOnly(2017, 11, 3),
				new PreferenceRestriction { WorkTimeLimitation = new WorkTimeLimitation(TimeSpan.FromHours(10), TimeSpan.FromHours(10)) }));

			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, period, new[] { agent }, preferenceDaysOrAss, skillDays);

			var result = Target.Execute(agent.VirtualSchedulePeriod(period.StartDate));
			result.Reason.Should().Be.EqualTo(RestrictionNotAbleToBeScheduledReason.NoIssue);

			Target2.Execute(new NoSchedulingCallback(), new SchedulingOptions(), new NoSchedulingProgress(), new[] { agent }, extendedPeriod);
			stateHolder.Schedules[agent].CalculatedContractTimeHolderOnPeriod(period).TotalHours.Should().Be.EqualTo(176);
		}

		[Test]
		public void ShouldCheckOnWeekMaxTimeAndIncludeFullWeekBefore()
		{
			var period = createStandardSetup(out var scenario, out var agent, out var skillDays);
			var extendedPeriod = new DateOnlyPeriod(period.StartDate, period.EndDate.AddDays(6));
			var preferenceDaysOrAss = new List<IPersistableScheduleData>();
			var shiftCategory = new ShiftCategory("_");
			var activity = new Activity().WithId();
			var firstWeekStartDate = new DateOnly(2017, 10, 30);
			for (int i = 0; i < 2; i++)
			{
				var ass = new PersonAssignment(agent, scenario, firstWeekStartDate.AddDays(i));
				ass.AddActivity(activity, new TimePeriod(8, 18));
				ass.SetShiftCategory(shiftCategory);
				preferenceDaysOrAss.Add(ass);
			}

			preferenceDaysOrAss.Add(new PreferenceDay(agent, new DateOnly(2017, 11, 1),
				new PreferenceRestriction { WorkTimeLimitation = new WorkTimeLimitation(TimeSpan.FromHours(10), TimeSpan.FromHours(10)) }));
			preferenceDaysOrAss.Add(new PreferenceDay(agent, new DateOnly(2017, 11, 2),
				new PreferenceRestriction { WorkTimeLimitation = new WorkTimeLimitation(TimeSpan.FromHours(10), TimeSpan.FromHours(10)) }));
			preferenceDaysOrAss.Add(new PreferenceDay(agent, new DateOnly(2017, 11, 3),
				new PreferenceRestriction { WorkTimeLimitation = new WorkTimeLimitation(TimeSpan.FromHours(10), TimeSpan.FromHours(10)) }));

			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, period, new[] { agent }, preferenceDaysOrAss, skillDays);

			var result = Target.Execute(agent.VirtualSchedulePeriod(period.StartDate));
			result.Reason.Should().Be.EqualTo(RestrictionNotAbleToBeScheduledReason.TooMuchWorkTimeInPeriod);
			result.Period.Should().Be
				.EqualTo(new DateOnlyPeriod(new DateOnly(2017, 10, 30), new DateOnly(2017, 10, 30).AddDays(6)));

			Target2.Execute(new NoSchedulingCallback(), new SchedulingOptions(), new NoSchedulingProgress(), new[] { agent }, extendedPeriod);
			stateHolder.Schedules[agent].CalculatedContractTimeHolderOnPeriod(period).TotalHours.Should().Be.LessThan(176);
		}

		[Test]
		public void ShouldFindNightRestIssues()
		{
			var period = createSetupWithFixedShiftLengths(out var scenario, out var agent, out var skillDays);
			var extendedPeriod = new DateOnlyPeriod(period.StartDate, period.EndDate.AddDays(6));
			var preferenceDaysOrAss = new List<IPersistableScheduleData>();

			agent.Period(period.StartDate).PersonContract.Contract.WorkTimeDirective = new WorkTimeDirective(TimeSpan.Zero,
				TimeSpan.FromHours(48), TimeSpan.FromHours(15), TimeSpan.FromHours(36));
			preferenceDaysOrAss.Add(new PreferenceDay(agent, new DateOnly(2017, 11, 1),
				new PreferenceRestriction { EndTimeLimitation = new EndTimeLimitation(TimeSpan.FromHours(18), TimeSpan.FromHours(18)) }));
			preferenceDaysOrAss.Add(new PreferenceDay(agent, new DateOnly(2017, 11, 2),
				new PreferenceRestriction { StartTimeLimitation = new StartTimeLimitation(TimeSpan.FromHours(8), TimeSpan.FromHours(8)) }));

			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, period, new[] { agent }, preferenceDaysOrAss, skillDays);

			var result = Target.Execute(agent.VirtualSchedulePeriod(period.StartDate));
			result.Reason.Should().Be.EqualTo(RestrictionNotAbleToBeScheduledReason.NightlyRestMightBeBroken);
			result.Period.Should().Be.EqualTo(new DateOnlyPeriod(2017, 11, 1, 2017, 11, 2));

			Target2.Execute(new NoSchedulingCallback(), new SchedulingOptions(), new NoSchedulingProgress(), new[] { agent }, extendedPeriod);
			stateHolder.Schedules[agent].CalculatedContractTimeHolderOnPeriod(period).TotalHours.Should().Be.LessThan(176);
		}

		[Test]
		public void ShouldCheckOnWeekMaxTimeAndIncludeFullWeekAfter()
		{
			var period = createStandardSetup(out var scenario, out var agent, out var skillDays);
			var extendedPeriod = new DateOnlyPeriod(period.StartDate, period.EndDate.AddDays(6));
			var preferenceDaysOrAss = new List<IPersistableScheduleData>();

			preferenceDaysOrAss.Add(new PreferenceDay(agent, new DateOnly(2017, 11, 27),
				new PreferenceRestriction { WorkTimeLimitation = new WorkTimeLimitation(TimeSpan.FromHours(10), TimeSpan.FromHours(10)) }));
			preferenceDaysOrAss.Add(new PreferenceDay(agent, new DateOnly(2017, 11, 28),
				new PreferenceRestriction { WorkTimeLimitation = new WorkTimeLimitation(TimeSpan.FromHours(10), TimeSpan.FromHours(10)) }));
			preferenceDaysOrAss.Add(new PreferenceDay(agent, new DateOnly(2017, 11, 29),
				new PreferenceRestriction { WorkTimeLimitation = new WorkTimeLimitation(TimeSpan.FromHours(10), TimeSpan.FromHours(10)) }));
			preferenceDaysOrAss.Add(new PreferenceDay(agent, new DateOnly(2017, 11, 30),
				new PreferenceRestriction { WorkTimeLimitation = new WorkTimeLimitation(TimeSpan.FromHours(10), TimeSpan.FromHours(10)) }));

			var ass = new PersonAssignment(agent, scenario, new DateOnly(2017, 12, 1));
			var activity = new Activity().WithId();
			ass.AddActivity(activity, new TimePeriod(8,18));
			ass.SetShiftCategory(new ShiftCategory("_"));
			preferenceDaysOrAss.Add(ass);

			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, period, new[] { agent }, preferenceDaysOrAss, skillDays);

			var result = Target.Execute(agent.VirtualSchedulePeriod(period.StartDate));
			result.Reason.Should().Be.EqualTo(RestrictionNotAbleToBeScheduledReason.TooMuchWorkTimeInPeriod);
			result.Period.Should().Be.EqualTo(new DateOnlyPeriod(new DateOnly(2017, 11, 27), new DateOnly(2017, 12, 03)));

			Target2.Execute(new NoSchedulingCallback(), new SchedulingOptions(), new NoSchedulingProgress(), new[] { agent }, extendedPeriod);
			stateHolder.Schedules[agent].CalculatedContractTimeHolderOnPeriod(period).TotalHours.Should().Be.LessThan(176);
		}

		private static DateOnlyPeriod createStandardSetup(out Scenario scenario, out Person agent, out IList<ISkillDay> skillDays)
		{
			var period = new DateOnlyPeriod(2017, 11, 01, 2017, 11, 30);
			var activity = new Activity().WithId();
			var skill = new Skill().For(activity).DefaultResolution(60).WithId().IsOpen();
			scenario = new Scenario();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity,
				new TimePeriodWithSegment(8, 0, 10, 0, 60), new TimePeriodWithSegment(16, 0, 18, 0, 60),
				new ShiftCategory("_").WithId()));
			agent = new Person().WithId()
				.WithPersonPeriod(new RuleSetBag(ruleSet), skill)
				.WithSchedulePeriodOneMonth(new DateOnly(2017, 10, 1));
			agent.Period(period.StartDate).PersonContract = new PersonContract(new Contract("_"), new PartTimePercentage("_"),
				new ContractScheduleWorkingMondayToFriday());
			skillDays = skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, period, 1);
			return period;
		}

		private static DateOnlyPeriod createSetupWithFixedShiftLengths(out Scenario scenario, out Person agent, out IList<ISkillDay> skillDays)
		{
			var period = new DateOnlyPeriod(2017, 11, 01, 2017, 11, 30);
			var activity = new Activity().WithId();
			var skill = new Skill().For(activity).DefaultResolution(60).WithId().IsOpen();
			scenario = new Scenario();
			var templateGenerator = new WorkShiftTemplateGenerator(activity,
				new TimePeriodWithSegment(8, 0, 10, 0, 60), new TimePeriodWithSegment(16, 0, 18, 0, 60),
				new ShiftCategory("_").WithId());
			var ruleSet = new WorkShiftRuleSet(templateGenerator);
			ruleSet.AddLimiter(new ContractTimeLimiter(new TimePeriod(8, 8), TimeSpan.FromHours(1)));
			agent = new Person().WithId()
				.WithPersonPeriod(new RuleSetBag(ruleSet), skill)
				.WithSchedulePeriodOneMonth(new DateOnly(2017, 10, 1));
			agent.Period(period.StartDate).PersonContract = new PersonContract(new Contract("_"), new PartTimePercentage("_"),
				new ContractScheduleWorkingMondayToFriday());
			skillDays = skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, period, 1);
			return period;
		}

		public RestrictionsAbleToBeScheduledOutsideSchedulingPeriodTest(ResourcePlannerTestParameters resourcePlannerTestParameters) : base(resourcePlannerTestParameters)
		{
		}
	}
}