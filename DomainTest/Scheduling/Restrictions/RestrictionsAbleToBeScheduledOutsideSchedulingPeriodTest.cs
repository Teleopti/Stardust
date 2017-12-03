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
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Restrictions
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
			var extendedPeriod = period.Extend(6);
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
			result.Should().Be.True();

			Target2.Execute(new NoSchedulingCallback(), new SchedulingOptions(), new NoSchedulingProgress(), new[] { agent }, extendedPeriod);
			stateHolder.Schedules[agent].CalculatedContractTimeHolderOnPeriod(period).TotalHours.Should().Be
				.EqualTo(176);
		}

		[Test]
		public void ShouldCheckOnWeekMaxTime()
		{
			var period = createStandardSetup(out var scenario, out var agent, out var skillDays);
			var extendedPeriod = period.Extend(6);
			var preferenceDays = new List<IPersistableScheduleData>();

			preferenceDays.Add(new PreferenceDay(agent, new DateOnly(2017, 12, 4), 
				new PreferenceRestriction { WorkTimeLimitation = new WorkTimeLimitation(TimeSpan.FromHours(9), TimeSpan.FromHours(9)) }));
			preferenceDays.Add(new PreferenceDay(agent, new DateOnly(2017, 12, 5),
				new PreferenceRestriction { WorkTimeLimitation = new WorkTimeLimitation(TimeSpan.FromHours(8), TimeSpan.FromHours(8)) }));
			preferenceDays.Add(new PreferenceDay(agent, new DateOnly(2017, 12, 6),
				new PreferenceRestriction { WorkTimeLimitation = new WorkTimeLimitation(TimeSpan.FromHours(8), TimeSpan.FromHours(8)) }));
			preferenceDays.Add(new PreferenceDay(agent, new DateOnly(2017, 12, 7),
				new PreferenceRestriction { WorkTimeLimitation = new WorkTimeLimitation(TimeSpan.FromHours(8), TimeSpan.FromHours(8)) }));
			preferenceDays.Add(new PreferenceDay(agent, new DateOnly(2017, 12, 8),
				new PreferenceRestriction { WorkTimeLimitation = new WorkTimeLimitation(TimeSpan.FromHours(8), TimeSpan.FromHours(8)) }));
			preferenceDays.Add(new PreferenceDay(agent, new DateOnly(2017, 12, 9),
				new PreferenceRestriction { WorkTimeLimitation = new WorkTimeLimitation(TimeSpan.FromHours(8), TimeSpan.FromHours(8)) }));

			preferenceDays.Add(new PreferenceDay(agent, new DateOnly(2017, 12, 10),
				new PreferenceRestriction { DayOffTemplate = new DayOffTemplate() }));

			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, period, new[] { agent }, preferenceDays, skillDays);

			var result = Target.Execute(agent.VirtualSchedulePeriod(period.StartDate));
			result.Should().Be.False();

			Target2.Execute(new NoSchedulingCallback(), new SchedulingOptions(), new NoSchedulingProgress(), new[] { agent }, extendedPeriod);
			stateHolder.Schedules[agent].CalculatedContractTimeHolderOnPeriod(period).TotalHours.Should().Be
				.EqualTo(0);
		}

		[Test]
		public void ShouldCheckOnWeekMaxTimeAndIncludeFullWeekBefore()
		{
			
		}

		[Test]
		public void ShouldCheckOnWeekMaxTimeAndIncludeFullWeekAfter()
		{

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
				.WithSchedulePeriodOneMonth(period.StartDate);
			agent.Period(period.StartDate).PersonContract = new PersonContract(new Contract("_"), new PartTimePercentage("_"),
				new ContractScheduleWorkingMondayToFriday());
			period = period.Extend(6);
			skillDays = skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, period, 1);
			return period;
		}

		public RestrictionsAbleToBeScheduledOutsideSchedulingPeriodTest(SeperateWebRequest seperateWebRequest, RemoveClassicShiftCategory resourcePlannerRemoveClassicShiftCat46582, RemoveImplicitResCalcContext removeImplicitResCalcContext46680) : base(seperateWebRequest, resourcePlannerRemoveClassicShiftCat46582, removeImplicitResCalcContext46680)
		{
		}
	}
}