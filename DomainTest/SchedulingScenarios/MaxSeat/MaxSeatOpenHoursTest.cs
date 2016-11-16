using System;
using System.Drawing;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.SeatLimitation;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.DomainTest.SchedulingScenarios.MaxSeat.TestData;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.MaxSeat
{
	[DomainTest]
	[Toggle(Toggles.ResourcePlanner_MaxSeatsNew_40939)]
	[TestFixture(TeamBlockType.Team)]
	[TestFixture(TeamBlockType.Block)]
	public class MaxSeatOpenHoursTest
	{
		private readonly TeamBlockType _teamBlockType;
		public MaxSeatOptimization Target;
		public GroupScheduleGroupPageDataProvider GroupScheduleGroupPageDataProvider;

		public MaxSeatOpenHoursTest(TeamBlockType teamBlockType)
		{
			_teamBlockType = teamBlockType;
		}

		[Test]
		public void ShouldConsiderOpenHours()
		{
			var site = new Site("_") { MaxSeats = 1 }.WithId();
			var team = new Team { Description = new Description("_"), Site = site };
			GroupScheduleGroupPageDataProvider.SetBusinessUnit_UseFromTestOnly(BusinessUnitFactory.CreateBusinessUnitAndAppend(team));
			var activity = new Activity("_") { RequiresSeat = true, RequiresSkill = true }.WithId();
			var skill = new Skill("_", "_", Color.AliceBlue, 15, new SkillTypePhone(new Description("_"), ForecastSource.InboundTelephony)) { Activity = activity };
			WorkloadFactory.CreateWorkloadWithOpenHours(skill, new TimePeriod(8, 24));
			var dateOnly = new DateOnly(2016, 10, 25);
			var scenario = new Scenario("_");
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(7, 0, 8, 0, 60), new TimePeriodWithSegment(15, 0, 16, 0, 60), new ShiftCategory("_").WithId()));
			ruleSet.AddLimiter(new ActivityTimeLimiter(activity, TimeSpan.FromHours(8), OperatorLimiter.Equals));
			var agentDataOneHour = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(16, 0, 17, 0));
			var agentData = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(9, 0, 17, 0));
			((PersonPeriod)agentData.Agent.Period(dateOnly)).AddPersonSkill(new PersonSkill(skill, new Percent(1)));
			var schedules = ScheduleDictionaryCreator.WithData(scenario, dateOnly.ToDateOnlyPeriod(), new[] { agentData.Assignment, agentDataOneHour.Assignment });
			var skillDay = skill.CreateSkillDayWithDemand(scenario, dateOnly, 0);

			Target.Optimize(dateOnly.ToDateOnlyPeriod(), new[] { agentData.Agent }, schedules, new[] { skillDay }, createOptimizationPreferences());

			schedules[agentData.Agent].ScheduledDay(dateOnly).PersonAssignment().Period.StartDateTime.TimeOfDay
				.Should().Be.EqualTo(TimeSpan.FromHours(8));
		}

		[Test]
		public void ShouldConsiderAgentTimeZoneWhenCheckingOpenHours()
		{
			var site = new Site("_") { MaxSeats = 1 }.WithId();
			var team = new Team { Description = new Description("_"), Site = site };
			GroupScheduleGroupPageDataProvider.SetBusinessUnit_UseFromTestOnly(BusinessUnitFactory.CreateBusinessUnitAndAppend(team));
			var activity = new Activity("_") { RequiresSeat = true, RequiresSkill = true }.WithId();
			var skill = new Skill("_", "_", Color.AliceBlue, 15, new SkillTypePhone(new Description("_"), ForecastSource.InboundTelephony)) { Activity = activity };
			WorkloadFactory.CreateWorkloadWithOpenHours(skill, new TimePeriod(8, 24));
			var dateOnly = new DateOnly(2016, 10, 25);
			var scenario = new Scenario("_");
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(7, 0, 8, 0, 60), new TimePeriodWithSegment(15, 0, 16, 0, 60), new ShiftCategory("_").WithId()));
			ruleSet.AddLimiter(new ActivityTimeLimiter(activity, TimeSpan.FromHours(8), OperatorLimiter.Equals));
			var agentDataOneHour = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(16, 0, 17, 0));
			var agentData = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(9, 0, 17, 0));
			((Person)agentData.Agent).InTimeZone(TimeZoneInfoFactory.StockholmTimeZoneInfo());
			((PersonPeriod)agentData.Agent.Period(dateOnly)).AddPersonSkill(new PersonSkill(skill, new Percent(1)));
			var schedules = ScheduleDictionaryCreator.WithData(scenario, dateOnly.ToDateOnlyPeriod(), new[] { agentData.Assignment, agentDataOneHour.Assignment });
			var skillDay = skill.CreateSkillDayWithDemand(scenario, dateOnly, 0);

			Target.Optimize(dateOnly.ToDateOnlyPeriod(), new[] { agentData.Agent }, schedules, new[] { skillDay }, createOptimizationPreferences());

			var time = TimeZoneHelper.ConvertFromUtc(schedules[agentData.Agent].ScheduledDay(dateOnly).PersonAssignment().Period.StartDateTime,
													TimeZoneInfoFactory.StockholmTimeZoneInfo());


			time.TimeOfDay.Should().Be.EqualTo(TimeSpan.FromHours(8));
		}

		[Test]
		public void ShouldConsiderClosedHours()
		{
			var site = new Site("_") { MaxSeats = 1 }.WithId();
			var team = new Team { Description = new Description("_"), Site = site };
			GroupScheduleGroupPageDataProvider.SetBusinessUnit_UseFromTestOnly(BusinessUnitFactory.CreateBusinessUnitAndAppend(team));
			var activity = new Activity("_") { RequiresSeat = true, RequiresSkill = true }.WithId();
			var skill = new Skill("_", "_", Color.AliceBlue, 15, new SkillTypePhone(new Description("_"), ForecastSource.InboundTelephony)) { Activity = activity };
			WorkloadFactory.CreateWorkloadWithOpenHours(skill, new TimePeriod(0, 7), new TimePeriod(8, 24));
			var dateOnly = new DateOnly(2016, 10, 25);
			var scenario = new Scenario("_");
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(7, 0, 8, 0, 60), new TimePeriodWithSegment(15, 0, 16, 0, 60), new ShiftCategory("_").WithId()));
			ruleSet.AddLimiter(new ActivityTimeLimiter(activity, TimeSpan.FromHours(8), OperatorLimiter.Equals));
			var agentDataOneHour = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(16, 0, 17, 0));
			var agentData = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(9, 0, 17, 0));
			((PersonPeriod)agentData.Agent.Period(dateOnly)).AddPersonSkill(new PersonSkill(skill, new Percent(1)));
			var schedules = ScheduleDictionaryCreator.WithData(scenario, dateOnly.ToDateOnlyPeriod(), new[] { agentData.Assignment, agentDataOneHour.Assignment });
			var skillDay = skill.CreateSkillDayWithDemand(scenario, dateOnly, 0);

			Target.Optimize(dateOnly.ToDateOnlyPeriod(), new[] { agentData.Agent }, schedules, new[] { skillDay }, createOptimizationPreferences());

			schedules[agentData.Agent].ScheduledDay(dateOnly).PersonAssignment().Period.StartDateTime.TimeOfDay
				.Should().Be.EqualTo(TimeSpan.FromHours(8));
		}

		[Test]
		public void ShouldConsiderIntersectingOpenHours()
		{
			var site = new Site("_") { MaxSeats = 1 }.WithId();
			var team = new Team { Description = new Description("_"), Site = site };
			GroupScheduleGroupPageDataProvider.SetBusinessUnit_UseFromTestOnly(BusinessUnitFactory.CreateBusinessUnitAndAppend(team));
			var activity = new Activity("_") { RequiresSeat = true }.WithId();
			var skill = new Skill("_", "_", Color.AliceBlue, 15, new SkillTypePhone(new Description("_"), ForecastSource.InboundTelephony)) { Activity = activity };
			WorkloadFactory.CreateWorkloadWithOpenHours(skill, new TimePeriod(0, 8), new TimePeriod(8, 24));
			var dateOnly = new DateOnly(2016, 10, 25);
			var scenario = new Scenario("_");
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(7, 0, 7, 0, 60), new TimePeriodWithSegment(15, 0, 15, 0, 60), new ShiftCategory("_").WithId()));
			ruleSet.AddLimiter(new ActivityTimeLimiter(activity, TimeSpan.FromHours(8), OperatorLimiter.Equals));
			var agentDataOneHour = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(16, 0, 17, 0));
			var agentData = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(9, 0, 17, 0));
			((PersonPeriod)agentData.Agent.Period(dateOnly)).AddPersonSkill(new PersonSkill(skill, new Percent(1)));
			var schedules = ScheduleDictionaryCreator.WithData(scenario, dateOnly.ToDateOnlyPeriod(), new[] { agentData.Assignment, agentDataOneHour.Assignment });
			var skillDay = skill.CreateSkillDayWithDemand(scenario, dateOnly, 0);

			Target.Optimize(dateOnly.ToDateOnlyPeriod(), new[] { agentData.Agent }, schedules, new[] { skillDay }, createOptimizationPreferences());

			schedules[agentData.Agent].ScheduledDay(dateOnly).PersonAssignment().Period.StartDateTime.TimeOfDay
				.Should().Be.EqualTo(TimeSpan.FromHours(7));
		}

		[Test]
		public void ShouldConsiderRequiresSkillWhenCheckingOpenHours()
		{
			var site = new Site("_") { MaxSeats = 1 }.WithId();
			var team = new Team { Description = new Description("_"), Site = site };
			GroupScheduleGroupPageDataProvider.SetBusinessUnit_UseFromTestOnly(BusinessUnitFactory.CreateBusinessUnitAndAppend(team));
			var activity = new Activity("_") { RequiresSeat = true, RequiresSkill = false }.WithId();
			var skillActivity = new Activity("_") { RequiresSeat = true }.WithId();
			var skill = new Skill("_", "_", Color.AliceBlue, 15, new SkillTypePhone(new Description("_"), ForecastSource.InboundTelephony)) { Activity = skillActivity };
			WorkloadFactory.CreateWorkloadWithOpenHours(skill, new TimePeriod(8, 24));
			var dateOnly = new DateOnly(2016, 10, 25);
			var scenario = new Scenario("_");
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(7, 0, 8, 0, 60), new TimePeriodWithSegment(15, 0, 16, 0, 60), new ShiftCategory("_").WithId()));
			ruleSet.AddLimiter(new ActivityTimeLimiter(activity, TimeSpan.FromHours(8), OperatorLimiter.Equals));
			var agentDataOneHour = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(16, 0, 17, 0));
			var agentData = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(9, 0, 17, 0));
			((PersonPeriod)agentData.Agent.Period(dateOnly)).AddPersonSkill(new PersonSkill(skill, new Percent(1)));
			var schedules = ScheduleDictionaryCreator.WithData(scenario, dateOnly.ToDateOnlyPeriod(), new[] { agentData.Assignment, agentDataOneHour.Assignment });
			var skillDay = skill.CreateSkillDayWithDemand(scenario, dateOnly, 0);

			Target.Optimize(dateOnly.ToDateOnlyPeriod(), new[] { agentData.Agent }, schedules, new[] { skillDay }, createOptimizationPreferences());

			schedules[agentData.Agent].ScheduledDay(dateOnly).PersonAssignment().Period.StartDateTime.TimeOfDay
				.Should().Be.EqualTo(TimeSpan.FromHours(7));
		}

		private OptimizationPreferences createOptimizationPreferences()
		{
			return DefaultMaxSeatOptimizationPreferences.Create(_teamBlockType);
		}
	}
}