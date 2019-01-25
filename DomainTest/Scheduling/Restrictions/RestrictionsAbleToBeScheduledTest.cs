using System;
using System.Collections.Generic;
using System.Linq;
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


namespace Teleopti.Ccc.DomainTest.Scheduling.Restrictions
{
	[DomainTest]
	[UseIocForFatClient]
	public class RestrictionsAbleToBeScheduledTest : SchedulingScenario
	{
		public RestrictionsAbleToBeScheduled Target;
		public DesktopScheduling Target2;
		public Func<ISchedulerStateHolder> SchedulerStateHolderFrom;

		[Test]
		//[Ignore("until 78216 is fixed")]
		public void ShouldAddMissingDaysOffAndReportTrueIfNoRestrictions()
		{
			var period = createStandardSetup(out var scenario, out var agent, out var skillDays);
			SchedulerStateHolderFrom.Fill(scenario, period, new[] {agent}, Enumerable.Empty<IPersonAssignment>(), skillDays);

			var result = Target.Execute(agent.VirtualSchedulePeriod(period.StartDate));
			result.Reason.Should().Be.EqualTo(RestrictionNotAbleToBeScheduledReason.NoRestrictions);
		}

		[Test]
		public void ShouldReportTrueIfRestrictionsProperlyEntered()
		{
			var period = createStandardSetup(out var scenario, out var agent, out var skillDays);
			var preferenceDays = new List<IPreferenceDay>();
			foreach (var dateOnly in period.DayCollection())
			{
				if (dateOnly.DayOfWeek == DayOfWeek.Saturday || dateOnly.DayOfWeek == DayOfWeek.Sunday)
					continue;

				preferenceDays.Add(new PreferenceDay(agent, dateOnly,
					new PreferenceRestriction { WorkTimeLimitation = new WorkTimeLimitation(TimeSpan.FromHours(8), TimeSpan.FromHours(8)) }));
			}

			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, period, new[] { agent }, preferenceDays, skillDays);

			var result = Target.Execute(agent.VirtualSchedulePeriod(period.StartDate));
			result.Reason.Should().Be.EqualTo(RestrictionNotAbleToBeScheduledReason.NoIssue);

			Target2.Execute(new NoSchedulingCallback(), new SchedulingOptions(), new NoSchedulingProgress(), new[] { agent }, period);
			stateHolder.Schedules[agent].CalculatedContractTimeHolderOnPeriod(period).Should().Be
				.EqualTo(TimeSpan.FromHours(168));
		}

		[Test]
		public void ShouldReportTrueIfRestrictionsProperlyEnteredAndSomeManualSchedulesHaveBeenEntered()
		{
			var period = createStandardSetup(out var scenario, out var agent, out var skillDays);
			var assignedOrPreferedDays = new List<IPersistableScheduleData>();
			foreach (var dateOnly in period.DayCollection())
			{
				if (dateOnly.DayOfWeek == DayOfWeek.Saturday || dateOnly.DayOfWeek == DayOfWeek.Sunday)
					continue;

				assignedOrPreferedDays.Add(new PreferenceDay(agent, dateOnly,
					new PreferenceRestriction { WorkTimeLimitation = new WorkTimeLimitation(TimeSpan.FromHours(8), TimeSpan.FromHours(8)) }));
			}

			var ass = new PersonAssignment(agent, scenario, period.StartDate);
			ass.SetDayOff(new DayOffTemplate());
			assignedOrPreferedDays.Add(ass);

			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, period, new[] { agent }, assignedOrPreferedDays, skillDays);

			var result = Target.Execute(agent.VirtualSchedulePeriod(period.StartDate));
			result.Reason.Should().Be.EqualTo(RestrictionNotAbleToBeScheduledReason.NoIssue);

			Target2.Execute(new NoSchedulingCallback(), new SchedulingOptions(), new NoSchedulingProgress(), new[] { agent }, period);
			stateHolder.Schedules[agent].CalculatedContractTimeHolderOnPeriod(period).Should().Be
				.EqualTo(TimeSpan.FromHours(168));
		}

		[Test]
		public void ShouldReportFalseIfWillWorkRestrictionOnAllDays()
		{
			var period = createStandardSetup(out var scenario, out var agent, out var skillDays);
			var preferenceDays = new List<IPreferenceDay>();
			foreach (var dateOnly in period.DayCollection())
			{
				preferenceDays.Add(new PreferenceDay(agent, dateOnly,
					new PreferenceRestriction { WorkTimeLimitation = new WorkTimeLimitation(TimeSpan.FromHours(8), null) }));
			}

			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, period, new[] { agent }, preferenceDays, skillDays);

			var result = Target.Execute(agent.VirtualSchedulePeriod(period.StartDate));
			result.Reason.Should().Be.EqualTo(RestrictionNotAbleToBeScheduledReason.TooMuchWorkTimeInPeriod);
			result.Period.Should().Be.EqualTo(period);

			Target2.Execute(new NoSchedulingCallback(), new SchedulingOptions(), new NoSchedulingProgress(), new[] { agent }, period);
			stateHolder.Schedules[agent].CalculatedContractTimeHolderOnPeriod(period).Should().Be
				.EqualTo(TimeSpan.Zero);
		}

		[Test]
		public void ShouldReportFalseIfRestrictionMinWorkTimeLongerThanTargetTime()
		{
			var period = createStandardSetup(out var scenario, out var agent, out var skillDays);
			var preferenceDays = new List<IPreferenceDay>();
			foreach (var dateOnly in period.DayCollection())
			{
				if(dateOnly.DayOfWeek == DayOfWeek.Saturday || dateOnly.DayOfWeek == DayOfWeek.Sunday)
					continue;

				preferenceDays.Add(new PreferenceDay(agent, dateOnly,
					new PreferenceRestriction { WorkTimeLimitation = new WorkTimeLimitation(TimeSpan.FromHours(10), TimeSpan.FromHours(10)) }));
			}

			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, period, new[] { agent }, preferenceDays, skillDays);

			var result = Target.Execute(agent.VirtualSchedulePeriod(period.StartDate));
			result.Reason.Should().Be.EqualTo(RestrictionNotAbleToBeScheduledReason.TooMuchWorkTimeInPeriod);
			result.Period.Should().Be.EqualTo(period);

			Target2.Execute(new NoSchedulingCallback(), new SchedulingOptions(), new NoSchedulingProgress(), new[] { agent }, period);
			stateHolder.Schedules[agent].CalculatedContractTimeHolderOnPeriod(period).Should().Be
				.EqualTo(TimeSpan.Zero);
		}

		[Test]
		public void ShouldReportFalseIfRestrictionMaxWorkTimeShorterThanTargetTime()
		{
			var period = createStandardSetup(out var scenario, out var agent, out var skillDays);
			var preferenceDays = new List<IPreferenceDay>();
			foreach (var dateOnly in period.DayCollection())
			{
				if (dateOnly.DayOfWeek == DayOfWeek.Saturday || dateOnly.DayOfWeek == DayOfWeek.Sunday)
					continue;

				preferenceDays.Add(new PreferenceDay(agent, dateOnly,
					new PreferenceRestriction { WorkTimeLimitation = new WorkTimeLimitation(TimeSpan.FromHours(4), TimeSpan.FromHours(4)) }));
			}

			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, period, new[] { agent }, preferenceDays, skillDays);

			var result = Target.Execute(agent.VirtualSchedulePeriod(period.StartDate));
			result.Reason.Should().Be.EqualTo(RestrictionNotAbleToBeScheduledReason.TooLittleWorkTimeInPeriod);
			result.Period.Should().Be.EqualTo(period);

			Target2.Execute(new NoSchedulingCallback(), new SchedulingOptions(), new NoSchedulingProgress(), new[] { agent }, period);
			stateHolder.Schedules[agent].CalculatedContractTimeHolderOnPeriod(period).Should().Be
				.EqualTo(TimeSpan.Zero);
		}

		[Test]
		public void ShouldHandleMoreThanOneCallToTargetWithDifferentPreferences()
		{
			var period = createStandardSetup(out var scenario, out var agent, out var skillDays);
			var preferenceDays = new List<IPreferenceDay>();
			foreach (var dateOnly in period.DayCollection())
			{
				if (dateOnly.DayOfWeek == DayOfWeek.Saturday || dateOnly.DayOfWeek == DayOfWeek.Sunday)
					continue;

				preferenceDays.Add(new PreferenceDay(agent, dateOnly,
					new PreferenceRestriction { WorkTimeLimitation = new WorkTimeLimitation(TimeSpan.FromHours(4), TimeSpan.FromHours(4)) }));
			}

			SchedulerStateHolderFrom.Fill(scenario, period, new[] { agent }, preferenceDays, skillDays);

			var result = Target.Execute(agent.VirtualSchedulePeriod(period.StartDate));
			result.Reason.Should().Be.EqualTo(RestrictionNotAbleToBeScheduledReason.TooLittleWorkTimeInPeriod);

			foreach (var dateOnly in period.DayCollection())
			{
				if (dateOnly.DayOfWeek == DayOfWeek.Saturday || dateOnly.DayOfWeek == DayOfWeek.Sunday)
					continue;

				preferenceDays.Add(new PreferenceDay(agent, dateOnly,
					new PreferenceRestriction { WorkTimeLimitation = new WorkTimeLimitation(TimeSpan.FromHours(8), TimeSpan.FromHours(8)) }));
			}

			SchedulerStateHolderFrom.Fill(scenario, period, new[] { agent }, preferenceDays, skillDays);

			result = Target.Execute(agent.VirtualSchedulePeriod(period.StartDate));
			result.Reason.Should().Be.EqualTo(RestrictionNotAbleToBeScheduledReason.NoIssue);
		}

		[Test]
		public void ShouldReportIfToManyDaysOff()
		{
			var period = createStandardSetup(out var scenario, out var agent, out var skillDays);
			var preferenceDays = new List<IPreferenceDay>();
			foreach (var dateOnly in period.DayCollection())
			{
				if (dateOnly.DayOfWeek == DayOfWeek.Saturday || dateOnly.DayOfWeek == DayOfWeek.Sunday || dateOnly == new DateOnly(2017, 12, 1))
				{
					preferenceDays.Add(new PreferenceDay(agent, dateOnly,
						new PreferenceRestriction { DayOffTemplate = new DayOffTemplate() }));
				}
			}

			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, period, new[] { agent }, preferenceDays, skillDays);

			var result = Target.Execute(agent.VirtualSchedulePeriod(period.StartDate));
			result.Reason.Should().Be.EqualTo(RestrictionNotAbleToBeScheduledReason.TooManyDaysOff);

			Target2.Execute(new NoSchedulingCallback(), new SchedulingOptions(), new NoSchedulingProgress(), new[] { agent }, period);
			stateHolder.Schedules[agent].CalculatedContractTimeHolderOnPeriod(period).Should().Be
				.EqualTo(TimeSpan.Zero);
		}

		[Test]
		public void ShouldHandleMoreDaysOffIfFlexibleDaysOff()
		{
			var period = createStandardSetupWithFlexibleContract(out var scenario, out var agent, out var skillDays);
			var preferenceDays = new List<IPreferenceDay>();
			foreach (var dateOnly in period.DayCollection())
			{
				if (dateOnly.DayOfWeek == DayOfWeek.Saturday || dateOnly.DayOfWeek == DayOfWeek.Sunday || dateOnly == new DateOnly(2017, 12, 1))
				{
					preferenceDays.Add(new PreferenceDay(agent, dateOnly,
						new PreferenceRestriction { DayOffTemplate = new DayOffTemplate()}));
				}
			}

			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, period, new[] { agent }, preferenceDays, skillDays);

			var result = Target.Execute(agent.VirtualSchedulePeriod(period.StartDate));
			result.Reason.Should().Be.EqualTo(RestrictionNotAbleToBeScheduledReason.NoIssue);

			Target2.Execute(new NoSchedulingCallback(), new SchedulingOptions(), new NoSchedulingProgress(), new[] { agent }, period);
			stateHolder.Schedules[agent].CalculatedContractTimeHolderOnPeriod(period).Should().Be
				.EqualTo(TimeSpan.FromHours(168));
		}

		[Test]
		public void ShouldHandleLessDaysOffIfFlexibleDaysOff()
		{
			var period = createStandardSetupWithFlexibleContract(out var scenario, out var agent, out var skillDays);
			var preferenceDays = new List<IPreferenceDay>();
			foreach (var dateOnly in period.DayCollection())
			{
				if ((dateOnly.DayOfWeek == DayOfWeek.Saturday || dateOnly.DayOfWeek == DayOfWeek.Sunday) && dateOnly != new DateOnly(2017, 12, 2))
				{
					preferenceDays.Add(new PreferenceDay(agent, dateOnly,
						new PreferenceRestriction { DayOffTemplate = new DayOffTemplate() }));
				}
				else
				{
					preferenceDays.Add(new PreferenceDay(agent, dateOnly,
						new PreferenceRestriction { WorkTimeLimitation = new WorkTimeLimitation(TimeSpan.FromHours(7), TimeSpan.FromHours(8)) }));
				}
			}

			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, period, new[] { agent }, preferenceDays, skillDays);

			var result = Target.Execute(agent.VirtualSchedulePeriod(period.StartDate));
			result.Reason.Should().Be.EqualTo(RestrictionNotAbleToBeScheduledReason.NoIssue);

			Target2.Execute(new NoSchedulingCallback(), new SchedulingOptions(), new NoSchedulingProgress(), new[] { agent }, period);
			stateHolder.Schedules[agent].CalculatedContractTimeHolderOnPeriod(period).Should().Be
				.EqualTo(TimeSpan.FromHours(168));
		}

		[Test]
		public void ShouldHandleMorePreferedWorkTimeIfFlexibleContractTime()
		{
			var period = createStandardSetupWithFlexibleContract(out var scenario, out var agent, out var skillDays);
			var preferenceDays = new List<IPreferenceDay>();
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
						new PreferenceRestriction { WorkTimeLimitation = new WorkTimeLimitation(TimeSpan.FromHours(9), TimeSpan.FromHours(9)) }));
				}
			}

			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, period, new[] { agent }, preferenceDays, skillDays);

			var result = Target.Execute(agent.VirtualSchedulePeriod(period.StartDate));
			result.Reason.Should().Be.EqualTo(RestrictionNotAbleToBeScheduledReason.NoIssue);

			Target2.Execute(new NoSchedulingCallback(), new SchedulingOptions(), new NoSchedulingProgress(), new[] { agent }, period);
			stateHolder.Schedules[agent].CalculatedContractTimeHolderOnPeriod(period).Should().Be
				.EqualTo(TimeSpan.FromHours(189));
		}

		[Test]
		public void ShouldHandleLessPreferedWorkTimeIfFlexibleContractTime()
		{
			var period = createStandardSetupWithFlexibleContract(out var scenario, out var agent, out var skillDays);
			var preferenceDays = new List<IPreferenceDay>();
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
						new PreferenceRestriction { WorkTimeLimitation = new WorkTimeLimitation(TimeSpan.FromHours(7), TimeSpan.FromHours(7)) }));
				}
			}

			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, period, new[] { agent }, preferenceDays, skillDays);

			var result = Target.Execute(agent.VirtualSchedulePeriod(period.StartDate));
			result.Reason.Should().Be.EqualTo(RestrictionNotAbleToBeScheduledReason.NoIssue);

			Target2.Execute(new NoSchedulingCallback(), new SchedulingOptions(), new NoSchedulingProgress(), new[] { agent }, period);
			stateHolder.Schedules[agent].CalculatedContractTimeHolderOnPeriod(period).Should().Be
				.EqualTo(TimeSpan.FromHours(147));
		}

		[Test]
		public void ShouldReportConflictingRestrictions()
		{
			var period = createStandardSetup(out var scenario, out var agent, out var skillDays);
			var shiftCategory = new ShiftCategory().WithId();
			var preferenceRestriction = new PreferenceRestriction { ShiftCategory = new ShiftCategory().WithId() };
			var preferenceDay = new PreferenceDay(agent, period.StartDate.AddDays(3), preferenceRestriction).WithId();
			var rotationRestriction = new RotationRestriction { ShiftCategory = shiftCategory };
			var rotationDay = new ScheduleDataRestriction(agent, rotationRestriction, period.StartDate.AddDays(3));

			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, period, new[] { agent }, new IScheduleData[] { rotationDay, preferenceDay }, skillDays);

			var result = Target.Execute(agent.VirtualSchedulePeriod(period.StartDate));
			result.Reason.Should().Be.EqualTo(RestrictionNotAbleToBeScheduledReason.ConflictingRestrictions);
			result.Period.Should().Be.EqualTo(new DateOnlyPeriod(period.StartDate.AddDays(3), period.StartDate.AddDays(3)));

			Target2.Execute(new NoSchedulingCallback(), new SchedulingOptions(), new NoSchedulingProgress(), new[] { agent }, period);
			stateHolder.Schedules[agent].ScheduledDay(period.StartDate.AddDays(3)).IsScheduled().Should().Be.False();
		}

		[Test]
		public void ShouldNotReportConflictingRestrictionsOnScheduledDay()
		{
			var period = createStandardSetup(out var scenario, out var agent, out var skillDays);
			var shiftCategory = new ShiftCategory().WithId();
			var preferenceRestriction = new PreferenceRestriction { ShiftCategory = new ShiftCategory().WithId() };
			var preferenceDay = new PreferenceDay(agent, period.StartDate.AddDays(3), preferenceRestriction).WithId();
			var rotationRestriction = new RotationRestriction { ShiftCategory = shiftCategory };
			var rotationDay = new ScheduleDataRestriction(agent, rotationRestriction, period.StartDate.AddDays(3));
			var activity = new Activity().WithId();
			var ass = new PersonAssignment(agent, scenario, period.StartDate.AddDays(3)).WithLayer(activity, new TimePeriod(8, 16)).ShiftCategory(shiftCategory);

			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, period, new[] { agent }, new IScheduleData[] { rotationDay, preferenceDay, ass }, skillDays);

			var result = Target.Execute(agent.VirtualSchedulePeriod(period.StartDate));
			result.Reason.Should().Be.EqualTo(RestrictionNotAbleToBeScheduledReason.NoIssue);

			Target2.Execute(new NoSchedulingCallback(), new SchedulingOptions(), new NoSchedulingProgress(), new[] { agent }, period);
			stateHolder.Schedules[agent].CalculatedContractTimeHolderOnPeriod(period).Should().Be
				.EqualTo(TimeSpan.FromHours(168));
		}

		[Test]
		public void ShouldNotReportBrokenNightRestBetweenTwoScheduledDay()
		{
			var period = createStandardSetup(out var scenario, out var agent, out var skillDays);
			var shiftCategory = new ShiftCategory().WithId();
			var activity = new Activity().WithId();
			var ass1 = new PersonAssignment(agent, scenario, period.StartDate).WithLayer(activity, new TimePeriod(15, 23)).ShiftCategory(shiftCategory);
			var ass2 = new PersonAssignment(agent, scenario, period.StartDate.AddDays(1)).WithLayer(activity, new TimePeriod(7, 15)).ShiftCategory(shiftCategory);
			SchedulerStateHolderFrom.Fill(scenario, period, new[] { agent }, new IScheduleData[] { ass1, ass2 }, skillDays);

			var result = Target.Execute(agent.VirtualSchedulePeriod(period.StartDate));
			result.Reason.Should().Be.EqualTo(RestrictionNotAbleToBeScheduledReason.NoRestrictions);

		}

		private static DateOnlyPeriod createStandardSetupWithFlexibleContract(out Scenario scenario, out Person agent, out IList<ISkillDay> skillDays)
		{
			var period = new DateOnlyPeriod(2017, 12, 01, 2017, 12, 31);
			var activity = new Activity().WithId();
			var skill = new Skill().For(activity).DefaultResolution(60).WithId().IsOpen();
			scenario = new Scenario();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity,
				new TimePeriodWithSegment(8, 0, 10, 0, 60), new TimePeriodWithSegment(16, 0, 18, 0, 60),
				new ShiftCategory("_").WithId()));
			agent = new Person().WithId()
				.WithPersonPeriod(new RuleSetBag(ruleSet), skill)
				.WithSchedulePeriodOneMonth(period.StartDate);
			agent.Period(period.StartDate).PersonContract = new PersonContract(new ContractWithMaximumTolerance(), new PartTimePercentage("_"),
				new ContractScheduleWorkingMondayToFriday());
			skillDays = skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, period, 1);
			return period;
		}

		private static DateOnlyPeriod createStandardSetup(out Scenario scenario, out Person agent, out IList<ISkillDay> skillDays)
		{
			var period = new DateOnlyPeriod(2017, 12, 01, 2017, 12, 31);
			var activity = new Activity().WithId();
			var skill = new Skill().For(activity).DefaultResolution(60).WithId().IsOpen();
			scenario = new Scenario();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity,
				new TimePeriodWithSegment(8, 0, 10, 0, 60), new TimePeriodWithSegment(16, 0, 18, 0, 60),
				new ShiftCategory("_").WithId()));
			agent = new Person().WithId()
				.WithPersonPeriod(new RuleSetBag(ruleSet), skill)
				.WithSchedulePeriodOneMonth(period.StartDate);
			agent.Period(period.StartDate).PersonContract = new PersonContract(new Contract("_"), new PartTimePercentage("_"),
				new ContractScheduleWorkingMondayToFriday());
			skillDays = skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, period, 1);
			return period;
		}

		public RestrictionsAbleToBeScheduledTest(ResourcePlannerTestParameters resourcePlannerTestParameters) : base(resourcePlannerTestParameters)
		{
		}
	}
}