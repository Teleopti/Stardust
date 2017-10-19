using System;
using System.Collections.Generic;
using NUnit.Framework;
using System.Linq;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourcePlanner.Validation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ResourcePlanner.Validation
{
	[DomainTest]
	[Toggle(Toggles.ResourcePlanner_ShowSwitchedTimeZone_46303)]
	public class ScheduleStartOnWrongDateValidatorTest
	{
		public Func<ISchedulerStateHolder> StateHolder; //just a way to be able to create a Ischeduledictionary... MAybe some easier way? Claes?
		public SchedulingValidator Target;

		[Test]
		public void ShouldJumpThroughIfScheduleIsNullToSupportCheapPreChecks()
		{
			Assert.DoesNotThrow(() =>
			{			
				Target.Validate(null, new[]{new Person(), }, DateOnly.Today.ToDateOnlyPeriod());
			});
		}

		[Test]
		[Ignore("to_be_continued")]
		public void ShouldReturnValidationErrorWhenAgentChangedToTimeZoneEarlier()
		{
			var scenario = new Scenario("_");
			var activity = ActivityFactory.CreateActivity("_");
			var dateOnly = new DateOnly(2010, 1, 1);
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(12, 0, 12, 0, 15), new TimePeriodWithSegment(21, 0, 21, 0, 15), new ShiftCategory("_").WithId()));
			var skill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
			var skillDay = skill.CreateSkillDayWithDemandPerHour(scenario, dateOnly, TimeSpan.FromMinutes(60), new List<Tuple<int, TimeSpan>>());
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet).WithSchedulePeriodOneDay(dateOnly);
			var ass = new PersonAssignment(agent, scenario, dateOnly).ShiftCategory(new ShiftCategory("_").WithId()).WithLayer(activity, new TimePeriod(8, 17));
			agent.PermissionInformation.SetDefaultTimeZone(TimeZoneInfoFactory.HawaiiTimeZoneInfo());
			var state = StateHolder.Fill(scenario, dateOnly, new[] { agent }, new IScheduleData[] { ass }, skillDay);
			
			var result = Target.Validate(state.Schedules, new[] {agent}, dateOnly.ToDateOnlyPeriod());
			
			result.InvalidResources.SelectMany(x => x.ValidationTypes)
				.Any(x => x == typeof(SchedulingValidator))
				.Should().Be.True();

		}
	}
}