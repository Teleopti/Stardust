﻿using System;
using System.Collections.Generic;
using System.Drawing;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Scheduling.SeatLimitation;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.IntradayOptimization
{
	[DomainTestWithStaticDependenciesAvoidUse]
	public class IntradayOptimizationTeamBlockDesktopTest
	{
		public IOptimizationCommand Target;
		public Func<ISchedulerStateHolder> SchedulerStateHolderFrom;
		public FakeBusinessUnitRepository BusinessUnitRepository;
		[RemoveMeWithToggle("Should not be necessary when toggle is on/removed", Toggles.ResourcePlanner_MaxSeatsNew_40939)]
		public IInitMaxSeatForStateHolder InitMaxSeatForStateHolder;

		[Test]
		public void ShouldNotCrashWhenUsingKeepExistingDaysOff()
		{
			BusinessUnitRepository.Has(ServiceLocatorForEntity.CurrentBusinessUnit.Current());
			var scenario = new Scenario("_");
			var phoneActivity = ActivityFactory.CreateActivity("_");
			var dateOnly = new DateOnly(2010, 1, 1);
			var contract = new Contract("_")
			{
				WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(36), TimeSpan.FromHours(63), TimeSpan.FromHours(11), TimeSpan.FromHours(36)),
				PositivePeriodWorkTimeTolerance = TimeSpan.FromHours(9)
			};
			var skill = new Skill("_", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = phoneActivity, TimeZone = TimeZoneInfo.Utc }.WithId();
			WorkloadFactory.CreateWorkloadWithFullOpenHours(skill);
			var skillDay = skill.CreateSkillDayWithDemandPerHour(scenario, dateOnly, TimeSpan.FromMinutes(60), new Tuple<int, TimeSpan>(17, TimeSpan.FromMinutes(360)));
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc);
			agent.AddPeriodWithSkill(new PersonPeriod(dateOnly, new PersonContract(contract, new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }), skill);
			agent.AddSchedulePeriod(new SchedulePeriod(dateOnly, SchedulePeriodType.Week, 1));
			agent.Period(dateOnly).RuleSetBag = new RuleSetBag(new WorkShiftRuleSet(new WorkShiftTemplateGenerator(phoneActivity, new TimePeriodWithSegment(8, 15, 8, 15, 15), new TimePeriodWithSegment(17, 15, 17, 15, 15), new ShiftCategory("_").WithId())));
			var ass = new PersonAssignment(agent, scenario, dateOnly);
			ass.AddActivity(phoneActivity, new TimePeriod(8, 0, 17, 0));
			var schedulerStateHolderFrom = SchedulerStateHolderFrom.Fill(scenario, dateOnly, new[] { agent }, new[] { ass }, skillDay);
			var optimizationPreferences = new OptimizationPreferencesDefaultValueProvider().Fetch();
			optimizationPreferences.General.OptimizationStepShiftsWithinDay = true;
			optimizationPreferences.Extra.UseTeams = true;
			var daysOffPreferences = new DaysOffPreferences {UseKeepExistingDaysOff = true};

			Assert.DoesNotThrow(() =>
			{
				Target.Execute(null,
					new NoSchedulingProgress(),
					schedulerStateHolderFrom,
					new List<IScheduleDay> { schedulerStateHolderFrom.Schedules[agent].ScheduledDay(dateOnly) },
					null,
					null,
					optimizationPreferences,
					false,
					daysOffPreferences,
					new FixedDayOffOptimizationPreferenceProvider(daysOffPreferences));
			});
		}

		[Test, Ignore("#40939 - först få detta grönt, sen köra detta både när vår toggle är på och av. sen kan vi härja liiiite säkrare")]
		public void ShouldRespectMaxSeatWhenIntradayOptimizationIsMade()
		{
			BusinessUnitRepository.Has(ServiceLocatorForEntity.CurrentBusinessUnit.Current());
			var site = new Site("_") { MaxSeats = 1 }.WithId();
			var team = new Team { Description = new Description("_"), Site = site };
			var activity = new Activity("_") { RequiresSeat = true }.WithId();
			var dateOnly = new DateOnly(2016, 10, 25);
			var scenario = new Scenario("_");
			//IS THIS NECESSARY?
			var skill = new Skill("_", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = activity, TimeZone = TimeZoneInfo.Utc }.WithId();
			WorkloadFactory.CreateWorkloadWithFullOpenHours(skill);
			var skillDay = skill.CreateSkillDayWithDemandOnInterval(scenario, dateOnly, 1, new Tuple<TimePeriod, double>(new TimePeriod(16, 0, 17, 0), 2));
			//??
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 60), new TimePeriodWithSegment(16, 0, 16, 0, 60), new ShiftCategory("_").WithId()));
			var agentScheduledOneHour = new Person().WithId().InTimeZone(TimeZoneInfo.Utc);
			agentScheduledOneHour.AddPersonPeriod(new PersonPeriod(dateOnly, new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), team));
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc);
			agent.AddPeriodWithSkill(new PersonPeriod(dateOnly, new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), team), skill);
			agent.AddSchedulePeriod(new SchedulePeriod(dateOnly, SchedulePeriodType.Day, 1));
			agent.Period(dateOnly).RuleSetBag = new RuleSetBag(ruleSet);
			var assOneHour = new PersonAssignment(agentScheduledOneHour, scenario, dateOnly);
			assOneHour.AddActivity(activity, new TimePeriod(16, 0, 17, 0));
			var ass = new PersonAssignment(agent, scenario, dateOnly);
			ass.AddActivity(activity, new TimePeriod(9, 0, 17, 0));
			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, dateOnly, new[] { agent }, new[] { ass, assOneHour }, skillDay);
			var optPreferences = new OptimizationPreferences
			{
				Extra =
				{
					UseTeamBlockOption = true,
					BlockTypeValue = BlockFinderType.SchedulePeriod,
					TeamGroupPage = new GroupPageLight("_", GroupPageType.SingleAgent)
				},
				Advanced = { UserOptionMaxSeatsFeature = MaxSeatsFeatureOptions.ConsiderMaxSeats },
				General = {ScheduleTag = new ScheduleTag(), OptimizationStepShiftsWithinDay = true}
			};
			InitMaxSeatForStateHolder.Execute(15);

			Target.Execute(null, new NoSchedulingProgress(), stateHolder, new [] { stateHolder.Schedules[agent].ScheduledDay(dateOnly) }, null, null, optPreferences, false, null, null);

			stateHolder.Schedules[agent].ScheduledDay(dateOnly).PersonAssignment().Period.StartDateTime.TimeOfDay
				.Should().Be.EqualTo(TimeSpan.FromHours(8));
		}
	}
}