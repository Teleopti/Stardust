﻿using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
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
	public class SchedulingTimeZoneTest : SchedulingScenario
	{
		public DesktopScheduling Target;
		public Func<ISchedulerStateHolder> SchedulerStateHolderFrom;
		public FakeTimeZoneGuard TimeZoneGuard; //this shouldn't effect scheduling at all, but it does currently....

		[TestCase(true)]
		[TestCase(false)]
		[Ignore("To be fixed #45670")]
		public void UserTimeZoneShouldNotAffectSchedulingOutcome(bool usersTimeZoneSameAsScheduledAgentsTimeZone)
		{
			TimeZoneGuard.SetTimeZone(usersTimeZoneSameAsScheduledAgentsTimeZone
				? TimeZoneInfoFactory.TaipeiTimeZoneInfo()
				: TimeZoneInfoFactory.UtcTimeZoneInfo());
			var date = new DateOnly(2017, 9, 7);
			var activity = new Activity("_").WithId();
			activity.RequiresSkill = true; //this was it! Claes! shouldn't it default to true!?
			var skill = new Skill().For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
			var scenario = new Scenario("_");
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(6, 0, 6, 0, 15), new TimePeriodWithSegment(14, 0, 14, 0, 15), new ShiftCategory("_").WithId()));
			var agent = new Person().WithId().InTimeZone(TimeZoneInfoFactory.TaipeiTimeZoneInfo()).WithPersonPeriod(ruleSet, new ContractWithMaximumTolerance(), skill).WithSchedulePeriodOneDay(date);
			var skillDays = skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, date.AddDays(-1), 1, 1, 1);
			var schedulerStateHolder = SchedulerStateHolderFrom.Fill(scenario, date.ToDateOnlyPeriod(), new[] { agent }, Enumerable.Empty<IPersonAssignment>(), skillDays);

			Target.Execute(new NoSchedulingCallback(), new SchedulingOptions(), new NoSchedulingProgress(), new[] { agent }, date.ToDateOnlyPeriod());

			schedulerStateHolder.Schedules[agent].ScheduledDay(date).IsScheduled().Should().Be.True();
		}

		public SchedulingTimeZoneTest(bool resourcePlannerMergeTeamblockClassicScheduling44289) : base(resourcePlannerMergeTeamblockClassicScheduling44289)
		{
		}
	}
}