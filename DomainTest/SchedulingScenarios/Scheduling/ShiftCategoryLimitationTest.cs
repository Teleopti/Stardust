using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.Scheduling
{
	[DomainTest]
	public class ShiftCategoryLimitationTest : SchedulingScenario
	{
		public DesktopScheduling Target;
		public Func<ISchedulerStateHolder> SchedulerStateHolderFrom;
		public GroupScheduleGroupPageDataProvider GroupScheduleGroupPageDataProvider;

		public ShiftCategoryLimitationTest(bool resourcePlannerTeamBlockPeriod42836) : base(resourcePlannerTeamBlockPeriod42836)
		{
		}

		[Test]
		[Ignore("#42836 (or maybe a seperate bug later)")]
		public void ShouldNotLeaveBlankSpotWhenAbleToSolve()
		{
			//REMOVEME!
			var team = new Team { Site = new Site("_") }.WithDescription(new Description("_"));
			GroupScheduleGroupPageDataProvider.SetBusinessUnit_UseFromTestOnly(BusinessUnitFactory.CreateBusinessUnitAndAppend(team));
			/////
			var date = new DateOnly(2017, 1, 22);
			var period = DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1);
			var shiftCat1 = new ShiftCategory("_").WithId();
			var shiftCat2 = new ShiftCategory("_").WithId();
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var skill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).IsOpen();
			var skillDays = skill.CreateSkillDayWithDemand(scenario, period, TimeSpan.FromHours(1));
			var ruleSet1 = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCat1));
			var ruleSet2 = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCat2));
			var ruleSetBag = new RuleSetBag(ruleSet1);
			ruleSetBag.AddRuleSet(ruleSet2);
			var agent = new Person().WithSchedulePeriodOneWeek(date).WithPersonPeriod(ruleSetBag, new ContractWithMaximumTolerance(),team, skill).InTimeZone(TimeZoneInfo.Utc);
			agent.SchedulePeriod(date).AddShiftCategoryLimitation(new ShiftCategoryLimitation(shiftCat1) { MaxNumberOf = 3 });
			agent.SchedulePeriod(date).AddShiftCategoryLimitation(new ShiftCategoryLimitation(shiftCat2) { MaxNumberOf = 3 });
			var stateholder = SchedulerStateHolderFrom.Fill(scenario, period, new[] { agent },
				new[]
				{
					new PersonAssignment(agent, scenario, date.AddDays(3)).IsDayOff(),
					new PersonAssignment(agent, scenario, date.AddDays(5)).IsDayOff()
				},
				skillDays
			);
			var optimizerOriginalPreferences = new OptimizerOriginalPreferences
			{
				SchedulingOptions =
				{
					UseBlock = true,
					UseShiftCategoryLimitations = true,
					BlockFinderTypeForAdvanceScheduling = BlockFinderType.BetweenDayOff,
					BlockSameShiftCategory = true
				}
			};

			Target.Execute(optimizerOriginalPreferences, new NoSchedulingProgress(), stateholder.Schedules.SchedulesForPeriod(period, agent), new OptimizationPreferences(), null);

			stateholder.Schedules[agent]
				.ScheduledDayCollection(period)
				.Count(x => x.PersonAssignment(true).MainActivities().Any())
				.Should()
				.Be.EqualTo(5);
		}

		[Test]
		public void ShouldRespectShiftCategoryLimitationWhenUsingTeamAndBlock()
		{
			var team = new Team { Site = new Site("_") }.WithDescription(new Description("_"));
			GroupScheduleGroupPageDataProvider.SetBusinessUnit_UseFromTestOnly(BusinessUnitFactory.CreateBusinessUnitAndAppend(team));
			var date = new DateOnly(2017, 1, 22);
			var period = DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1);
			var shiftCat1 = new ShiftCategory("1").WithId();
			var shiftCat2 = new ShiftCategory("2").WithId();
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var skill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).IsOpen();
			var skillDays = skill.CreateSkillDayWithDemand(scenario, period, TimeSpan.FromHours(1));
			var ruleSet1 = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCat1));
			var ruleSet2 = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCat2));
			var ruleSetBag = new RuleSetBag(ruleSet1);
			ruleSetBag.AddRuleSet(ruleSet2);
			var agent1 = new Person().WithSchedulePeriodOneWeek(date).WithPersonPeriod(ruleSetBag, new ContractWithMaximumTolerance(), team, skill).InTimeZone(TimeZoneInfo.Utc);
			var agent2 = new Person().WithSchedulePeriodOneWeek(date).WithPersonPeriod(ruleSetBag, new ContractWithMaximumTolerance(), team, skill).InTimeZone(TimeZoneInfo.Utc);
			agent1.SchedulePeriod(date).AddShiftCategoryLimitation(new ShiftCategoryLimitation(shiftCat1) { MaxNumberOf = 0 });
			agent2.SchedulePeriod(date).AddShiftCategoryLimitation(new ShiftCategoryLimitation(shiftCat2) { MaxNumberOf = 0 });
			var stateholder = SchedulerStateHolderFrom.Fill(scenario, period,  new[] { agent1, agent2 }, 
					new []
					{
						new PersonAssignment(agent1, scenario, date).IsDayOff(),
						new PersonAssignment(agent2, scenario, date).IsDayOff(),
						new PersonAssignment(agent1, scenario, date.AddDays(2)).IsDayOff(),
						new PersonAssignment(agent2, scenario, date.AddDays(2)).IsDayOff()
					},
					skillDays
			);
			var optimizerOriginalPreferences = new OptimizerOriginalPreferences
			{
				SchedulingOptions =
				{
					GroupOnGroupPageForTeamBlockPer = new GroupPageLight("_", GroupPageType.Hierarchy),
					UseTeam = true,
					UseBlock = true,
					UseShiftCategoryLimitations = true,
					BlockFinderTypeForAdvanceScheduling = BlockFinderType.BetweenDayOff,
					BlockSameShiftCategory = true,
					TeamSameShiftCategory = true
				}
			};

			Target.Execute(optimizerOriginalPreferences, new NoSchedulingProgress(), stateholder.Schedules.SchedulesForPeriod(period, agent1, agent2), new OptimizationPreferences(), null);

			foreach (var day in new[]{date.AddDays(1), date.AddDays(3), date.AddDays(4), date.AddDays(5), date.AddDays(6)})
			{
				var scheduledDays = stateholder.Schedules.SchedulesForDay(day);
				scheduledDays.Count(x => x.PersonAssignment(true).MainActivities().Any()).Should().Be.EqualTo(1);
				scheduledDays.Count(x => !x.PersonAssignment(true).MainActivities().Any()).Should().Be.EqualTo(1);
			}
		}

		[Test]
		public void ShouldRespectShiftCategoryLimitationWhenUsingTeamAndBlockAndNotPossibleToScheduleFullBlockPeriod()
		{
			var team = new Team { Site = new Site("_") }.WithDescription(new Description("_"));
			GroupScheduleGroupPageDataProvider.SetBusinessUnit_UseFromTestOnly(BusinessUnitFactory.CreateBusinessUnitAndAppend(team));
			var date = new DateOnly(2017, 1, 22);
			var period = DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1);
			var shiftCat1 = new ShiftCategory("1").WithId();
			var shiftCat2 = new ShiftCategory("2").WithId();
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var skill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).IsOpen();
			var skillDays = skill.CreateSkillDayWithDemand(scenario, period, TimeSpan.FromHours(1));
			var ruleSet1 = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCat1));
			var ruleSet2 = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCat2));
			var ruleSetBag = new RuleSetBag(ruleSet1);
			ruleSetBag.AddRuleSet(ruleSet2);
			var agent1 = new Person().WithSchedulePeriodOneWeek(date).WithPersonPeriod(ruleSetBag, new ContractWithMaximumTolerance(), team, skill).InTimeZone(TimeZoneInfo.Utc);
			var agent2 = new Person().WithSchedulePeriodOneWeek(date).WithPersonPeriod(ruleSetBag, new ContractWithMaximumTolerance(), team, skill).InTimeZone(TimeZoneInfo.Utc);
			agent1.SchedulePeriod(date).AddShiftCategoryLimitation(new ShiftCategoryLimitation(shiftCat1) { MaxNumberOf = 1 });
			agent1.SchedulePeriod(date).AddShiftCategoryLimitation(new ShiftCategoryLimitation(shiftCat2) { MaxNumberOf = 1 });
			agent2.SchedulePeriod(date).AddShiftCategoryLimitation(new ShiftCategoryLimitation(shiftCat1) { MaxNumberOf = 1 });
			agent2.SchedulePeriod(date).AddShiftCategoryLimitation(new ShiftCategoryLimitation(shiftCat2) { MaxNumberOf = 1 });
			var stateholder = SchedulerStateHolderFrom.Fill(scenario, period, new[] { agent1, agent2 },
					new[]
					{
						new PersonAssignment(agent1, scenario, date).IsDayOff(),
						new PersonAssignment(agent2, scenario, date).IsDayOff(),
						new PersonAssignment(agent1, scenario, date.AddDays(3)).IsDayOff(),
						new PersonAssignment(agent2, scenario, date.AddDays(3)).IsDayOff()
					},
					skillDays
			);
			var optimizerOriginalPreferences = new OptimizerOriginalPreferences
			{
				SchedulingOptions =
				{
					GroupOnGroupPageForTeamBlockPer = new GroupPageLight("_", GroupPageType.Hierarchy),
					UseTeam = true,
					UseBlock = true,
					UseShiftCategoryLimitations = true,
					BlockFinderTypeForAdvanceScheduling = BlockFinderType.BetweenDayOff,
					BlockSameShiftCategory = true,
					TeamSameShiftCategory = true
				}
			};


			var blockPeriod = new DateOnlyPeriod(date, date.AddDays(3));
			Target.Execute(optimizerOriginalPreferences, new NoSchedulingProgress(), stateholder.Schedules.SchedulesForPeriod(blockPeriod, agent1, agent2), new OptimizationPreferences(), null);

			stateholder.Schedules.SchedulesForPeriod(period, agent1, agent2)
				.Count(x => x.PersonAssignment(true).MainActivities().Any())
				.Should().Be.LessThanOrEqualTo(2);
		}

		[Test]
		public void ShouldNotTouchTeamMembersNotInSelectionWhenUsingTeamAndBlock()
		{
			var team = new Team { Site = new Site("_") }.WithDescription(new Description("_"));
			GroupScheduleGroupPageDataProvider.SetBusinessUnit_UseFromTestOnly(BusinessUnitFactory.CreateBusinessUnitAndAppend(team));
			var date = new DateOnly(2017, 1, 22);
			var period = new DateOnlyPeriod(date, date.AddDays(2));
			var shiftCategoryBefore = new ShiftCategory("Before").WithId();
			var shiftCategoryAfter = new ShiftCategory("After").WithId();
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var skill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).IsOpen();
			var skillDay = skill.CreateSkillDayWithDemand(scenario, date.AddDays(1), 2);
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(14, 0, 14, 0, 15), new TimePeriodWithSegment(22, 0, 22, 0, 15), shiftCategoryAfter));
			var selectedAgent = new Person().WithSchedulePeriodOneWeek(date).WithPersonPeriod(ruleSet, team, skill).InTimeZone(TimeZoneInfo.Utc);
			var agentNotInSelection = new Person().WithSchedulePeriodOneWeek(date).WithPersonPeriod(ruleSet, team, skill).InTimeZone(TimeZoneInfo.Utc);
			selectedAgent.SchedulePeriod(date).AddShiftCategoryLimitation(new ShiftCategoryLimitation(shiftCategoryBefore) { MaxNumberOf = 0 });
			agentNotInSelection.SchedulePeriod(date).AddShiftCategoryLimitation(new ShiftCategoryLimitation(shiftCategoryBefore) { MaxNumberOf = 0 });
			var stateholder = SchedulerStateHolderFrom.Fill(scenario, period, new[] { selectedAgent, agentNotInSelection}, new[]
			{
				new PersonAssignment(selectedAgent, scenario, date).IsDayOff(),
				new PersonAssignment(selectedAgent, scenario, date.AddDays(1)).ShiftCategory(shiftCategoryBefore).WithLayer(activity, new TimePeriod(6, 14)),
				new PersonAssignment(selectedAgent, scenario, date.AddDays(2)).IsDayOff(),
				new PersonAssignment(selectedAgent, scenario, date).IsDayOff(),
				new PersonAssignment(selectedAgent, scenario, date.AddDays(2)).IsDayOff()
			}, skillDay);
			var optimizerOriginalPreferences = new OptimizerOriginalPreferences
			{
				SchedulingOptions =
				{
					GroupOnGroupPageForTeamBlockPer = new GroupPageLight("_", GroupPageType.Hierarchy),
					UseTeam = true,
					UseBlock = true,
					UseShiftCategoryLimitations = true,
					BlockFinderTypeForAdvanceScheduling = BlockFinderType.BetweenDayOff,
					BlockSameShiftCategory = true,
					TeamSameShiftCategory = true,
					AllowBreakContractTime = true
				}
			};

			Target.Execute(optimizerOriginalPreferences, new NoSchedulingProgress(), stateholder.Schedules.SchedulesForPeriod(period, selectedAgent), new OptimizationPreferences(), null);

			stateholder.Schedules[selectedAgent].ScheduledDay(date.AddDays(1)).PersonAssignment().ShiftCategory.Should().Be.EqualTo(shiftCategoryAfter);
			stateholder.Schedules[agentNotInSelection].ScheduledDay(date.AddDays(1)).PersonAssignment(true).ShiftLayers.Count().Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldTryToReplaceSecondShiftIfFirstWasUnsuccessful()
		{
			var firstDate = new DateOnly(2017, 1, 22);
			var secondDate = firstDate.AddDays(1);
			var shiftCategoryBefore = new ShiftCategory("Before").WithId();
			var shiftCategoryAfter = new ShiftCategory("After").WithId();
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var nightRest = TimeSpan.FromHours(11);
			var contract = new Contract("_") { WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(10), TimeSpan.FromHours(83), nightRest, TimeSpan.FromHours(16)) };
			var skill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).IsOpen();
			var skillDayFirstDay = skill.CreateSkillDayWithDemand(scenario, firstDate, 1); //should try with this one first
			var skillDaySecondDay = skill.CreateSkillDayWithDemand(scenario, secondDate, 10); 
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(14, 0, 14, 0, 15), new TimePeriodWithSegment(22, 0, 22, 0, 15), shiftCategoryAfter));
			var agent = new Person().WithSchedulePeriodOneWeek(firstDate).WithPersonPeriod(ruleSet, contract, skill).InTimeZone(TimeZoneInfo.Utc);
			agent.SchedulePeriod(firstDate).AddShiftCategoryLimitation(new ShiftCategoryLimitation(shiftCategoryBefore) { MaxNumberOf = 1 });
			var period = new DateOnlyPeriod(firstDate, secondDate);
			var assA = new PersonAssignment(agent, scenario, firstDate).ShiftCategory(shiftCategoryBefore).WithLayer(activity, new TimePeriod(6, 14));
			var assB = new PersonAssignment(agent, scenario, secondDate).ShiftCategory(shiftCategoryBefore).WithLayer(activity, new TimePeriod(6, 14));
			var stateholder = SchedulerStateHolderFrom.Fill(scenario, period, new[] { agent }, new[] { assA, assB }, new[] { skillDayFirstDay, skillDaySecondDay });

			Target.Execute(createOptimizerOriginalPreferencesTeamSingleAgent(), new NoSchedulingProgress(), stateholder.Schedules[agent].ScheduledDayCollection(period), new OptimizationPreferences(), null);

			stateholder.Schedules[agent].ScheduledDay(firstDate).PersonAssignment().ShiftCategory.Should().Be.EqualTo(shiftCategoryBefore);
			stateholder.Schedules[agent].ScheduledDay(secondDate).PersonAssignment().ShiftCategory.Should().Be.EqualTo(shiftCategoryAfter);
		}

		[Test]
		public void ShouldRemoveShiftToFulfillLimitation()
		{
			var date = new DateOnly(2017, 1, 22);
			var shiftCategory = new ShiftCategory("_").WithId();
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var contract = new Contract("_") { WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(10), TimeSpan.FromHours(83), TimeSpan.FromHours(11), TimeSpan.FromHours(16)) };
			var skill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).IsOpen();
			var skillDay = skill.CreateSkillDayWithDemand(scenario, date, 1);
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(14, 0, 14, 0, 15), new TimePeriodWithSegment(22, 0, 22, 0, 15), shiftCategory));
			var agent = new Person().WithSchedulePeriodOneWeek(date).WithPersonPeriod(ruleSet, contract, skill).InTimeZone(TimeZoneInfo.Utc);
			agent.SchedulePeriod(date).AddShiftCategoryLimitation(new ShiftCategoryLimitation(shiftCategory) { MaxNumberOf = 1 });
			var period = new DateOnlyPeriod(date, date);
			var assA = new PersonAssignment(agent, scenario, date).ShiftCategory(shiftCategory).WithLayer(activity, new TimePeriod(6, 14));
			var assB = new PersonAssignment(agent, scenario, date.AddDays(1)).ShiftCategory(shiftCategory).WithLayer(activity, new TimePeriod(6, 14));
			var stateholder = SchedulerStateHolderFrom.Fill(scenario, period, new[] { agent }, new[] { assA, assB}, new[] { skillDay});

			Target.Execute(createOptimizerOriginalPreferencesTeamSingleAgent(), new NoSchedulingProgress(), stateholder.Schedules[agent].ScheduledDayCollection(period), new OptimizationPreferences(), null);

			stateholder.Schedules[agent].ScheduledDay(date).PersonAssignment(true).ShiftLayers.Should().Be.Empty();
			stateholder.Schedules[agent].ScheduledDay(date.AddDays(1)).PersonAssignment().ShiftCategory.Should().Be.EqualTo(shiftCategory);
		}

		[Test]
		public void ShouldReplaceBestDayFirst()
		{
			var firstDate = new DateOnly(2017, 1, 22);
			var secondDate = firstDate.AddDays(1);
			var shiftCategoryBefore = new ShiftCategory("Before").WithId();
			var shiftCategoryAfter = new ShiftCategory("After").WithId();
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var contract = new Contract("_") { WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(10), TimeSpan.FromHours(83), TimeSpan.FromHours(1), TimeSpan.FromHours(16)) };
			var skill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).IsOpen();
			var skillDayFirstDay = skill.CreateSkillDayWithDemand(scenario, firstDate, 10); 
			var skillDaySecondDay = skill.CreateSkillDayWithDemand(scenario, secondDate, 1); //should try with this one first
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(14, 0, 14, 0, 15), new TimePeriodWithSegment(22, 0, 22, 0, 15), shiftCategoryAfter));
			var agent = new Person().WithSchedulePeriodOneWeek(firstDate).WithPersonPeriod(ruleSet, contract, skill).InTimeZone(TimeZoneInfo.Utc);
			agent.SchedulePeriod(firstDate).AddShiftCategoryLimitation(new ShiftCategoryLimitation(shiftCategoryBefore) { MaxNumberOf = 1 });
			var period = new DateOnlyPeriod(firstDate, secondDate);
			var assA = new PersonAssignment(agent, scenario, firstDate).ShiftCategory(shiftCategoryBefore).WithLayer(activity, new TimePeriod(6, 14));
			var assB = new PersonAssignment(agent, scenario, secondDate).ShiftCategory(shiftCategoryBefore).WithLayer(activity, new TimePeriod(6, 14));
			var stateholder = SchedulerStateHolderFrom.Fill(scenario, period, new[] { agent }, new[] { assA, assB }, new[] { skillDayFirstDay, skillDaySecondDay });

			Target.Execute(createOptimizerOriginalPreferencesTeamSingleAgent(), new NoSchedulingProgress(), stateholder.Schedules[agent].ScheduledDayCollection(period), new OptimizationPreferences(), null);

			stateholder.Schedules[agent].ScheduledDay(firstDate).PersonAssignment().ShiftCategory.Should().Be.EqualTo(shiftCategoryBefore);
			stateholder.Schedules[agent].ScheduledDay(secondDate).PersonAssignment().ShiftCategory.Should().Be.EqualTo(shiftCategoryAfter);
		}

		[Test]
		public void ShouldTryToReplaceSecondShiftIfFirstWasUnsuccessful_MultipleAgents()
		{
			var firstDate = new DateOnly(2017, 1, 22);
			var secondDate = firstDate.AddDays(1);
			var period = new DateOnlyPeriod(firstDate, secondDate);
			var shiftCategoryBefore = new ShiftCategory("Before").WithId();
			var shiftCategoryAfter = new ShiftCategory("After").WithId();
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var nightRest = TimeSpan.FromHours(11);
			var contract = new Contract("_") { WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(10), TimeSpan.FromHours(83), nightRest, TimeSpan.FromHours(16)) };
			var skill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).IsOpen();
			var skillDayFirstDay = skill.CreateSkillDayWithDemand(scenario, firstDate, 2); //should try with this one first, but can't because of night rest
			var skillDaySecondDay = skill.CreateSkillDayWithDemand(scenario, secondDate, 10);
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(14, 0, 14, 0, 15), new TimePeriodWithSegment(22, 0, 22, 0, 15), shiftCategoryAfter));
			var firstAgent = new Person().WithName(new Name("first", "first")).WithSchedulePeriodOneWeek(firstDate).WithPersonPeriod(ruleSet, contract, skill).InTimeZone(TimeZoneInfo.Utc);
			var secondAgent = new Person().WithName(new Name("second", "second")).WithSchedulePeriodOneWeek(firstDate).WithPersonPeriod(ruleSet, contract, skill).InTimeZone(TimeZoneInfo.Utc);
			firstAgent.SchedulePeriod(firstDate).AddShiftCategoryLimitation(new ShiftCategoryLimitation(shiftCategoryBefore) { MaxNumberOf = 1 });
			secondAgent.SchedulePeriod(firstDate).AddShiftCategoryLimitation(new ShiftCategoryLimitation(shiftCategoryBefore) { MaxNumberOf = 1 });
			var firstAgentAssA = new PersonAssignment(firstAgent, scenario, firstDate).ShiftCategory(shiftCategoryBefore).WithLayer(activity, new TimePeriod(6, 14));
			var firstAgentAssB = new PersonAssignment(firstAgent, scenario, secondDate).ShiftCategory(shiftCategoryBefore).WithLayer(activity, new TimePeriod(6, 14));
			var secondAgentAssA = new PersonAssignment(secondAgent, scenario, firstDate).ShiftCategory(shiftCategoryBefore).WithLayer(activity, new TimePeriod(6, 14));
			var secondAgentAssB = new PersonAssignment(secondAgent, scenario, secondDate).ShiftCategory(shiftCategoryBefore).WithLayer(activity, new TimePeriod(6, 14));
			var stateholder = SchedulerStateHolderFrom.Fill(scenario, period, new[] { firstAgent, secondAgent }, new[] { firstAgentAssA, firstAgentAssB, secondAgentAssA, secondAgentAssB }, new[] { skillDayFirstDay, skillDaySecondDay });

			Target.Execute(createOptimizerOriginalPreferencesTeamSingleAgent(), new NoSchedulingProgress(), stateholder.Schedules.SchedulesForPeriod(period, firstAgent, secondAgent), new OptimizationPreferences(), null);

			stateholder.Schedules.SchedulesForDay(firstDate).All(x => x.PersonAssignment().ShiftCategory.Equals(shiftCategoryBefore))
				.Should().Be.True();
			stateholder.Schedules.SchedulesForDay(secondDate).All(x => x.PersonAssignment().ShiftCategory.Equals(shiftCategoryAfter))
				.Should().Be.True();
		}

		[Test]
		public void ShouldSetCorrectTag()
		{
			var firstDate = new DateOnly(2017, 1, 22);
			var secondDate = firstDate.AddDays(1);
			var shiftCategoryBefore = new ShiftCategory("Before").WithId();
			var shiftCategoryAfter = new ShiftCategory("After").WithId();
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var nightRest = TimeSpan.FromHours(1);
			var contract = new Contract("_") { WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(10), TimeSpan.FromHours(83), nightRest, TimeSpan.FromHours(16)) };
			var skill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).IsOpen();
			var skillDayFirstDay = skill.CreateSkillDayWithDemand(scenario, firstDate, 1);
			var skillDaySecondDay = skill.CreateSkillDayWithDemand(scenario, secondDate, 10);
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(14, 0, 14, 0, 15), new TimePeriodWithSegment(22, 0, 22, 0, 15), shiftCategoryAfter));
			var tag = new ScheduleTag();
			var optimizerOriginalPreferences = createOptimizerOriginalPreferencesTeamSingleAgent();
			optimizerOriginalPreferences.SchedulingOptions.TagToUseOnScheduling = tag;
			var agent = new Person().WithSchedulePeriodOneWeek(firstDate).WithPersonPeriod(ruleSet, contract, skill).InTimeZone(TimeZoneInfo.Utc);
			agent.SchedulePeriod(firstDate).AddShiftCategoryLimitation(new ShiftCategoryLimitation(shiftCategoryBefore) { MaxNumberOf = 1 });
			var period = new DateOnlyPeriod(firstDate, secondDate);
			var assFirstDate = new PersonAssignment(agent, scenario, firstDate).ShiftCategory(shiftCategoryBefore).WithLayer(activity, new TimePeriod(6, 14));
			var assSecondDate = new PersonAssignment(agent, scenario, secondDate).ShiftCategory(shiftCategoryBefore).WithLayer(activity, new TimePeriod(6, 14));
			var stateholder = SchedulerStateHolderFrom.Fill(scenario, period, new[] { agent }, new IScheduleData[] { assFirstDate, assSecondDate }, new[] { skillDayFirstDay, skillDaySecondDay });

			Target.Execute(optimizerOriginalPreferences, new NoSchedulingProgress(), stateholder.Schedules[agent].ScheduledDayCollection(period), new OptimizationPreferences(), null);

			stateholder.Schedules[agent].ScheduledDay(firstDate).ScheduleTag()
				.Should().Be.SameInstanceAs(tag);
		}

		[Test]
		public void ShouldKeepTagWhenReplacingShift()
		{
			var firstDate = new DateOnly(2017, 1, 22);
			var secondDate = firstDate.AddDays(1);
			var shiftCategoryBefore = new ShiftCategory("Before").WithId();
			var shiftCategoryAfter = new ShiftCategory("After").WithId();
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var nightRest = TimeSpan.FromHours(1);
			var contract = new Contract("_") { WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(10), TimeSpan.FromHours(83), nightRest, TimeSpan.FromHours(16)) };
			var skill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).IsOpen();
			var skillDayFirstDay = skill.CreateSkillDayWithDemand(scenario, firstDate, 1);
			var skillDaySecondDay = skill.CreateSkillDayWithDemand(scenario, secondDate, 10);
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(14, 0, 14, 0, 15), new TimePeriodWithSegment(22, 0, 22, 0, 15), shiftCategoryAfter));
			var tag = new ScheduleTag();
			var optimizerOriginalPreferences = createOptimizerOriginalPreferencesTeamSingleAgent();
			optimizerOriginalPreferences.SchedulingOptions.TagToUseOnScheduling = tag;
			var agent = new Person().WithSchedulePeriodOneWeek(firstDate).WithPersonPeriod(ruleSet, contract, skill).InTimeZone(TimeZoneInfo.Utc);
			agent.SchedulePeriod(firstDate).AddShiftCategoryLimitation(new ShiftCategoryLimitation(shiftCategoryBefore) { MaxNumberOf = 1 });
			var period = new DateOnlyPeriod(firstDate, secondDate);
			var assFirstDate = new PersonAssignment(agent, scenario, firstDate).ShiftCategory(shiftCategoryBefore).WithLayer(activity, new TimePeriod(6, 14));
			var assSecondDate = new PersonAssignment(agent, scenario, secondDate).ShiftCategory(shiftCategoryBefore).WithLayer(activity, new TimePeriod(6, 14));
			var tagFirstDate = new AgentDayScheduleTag(agent, firstDate, scenario, tag);
			var tagSecondDate = new AgentDayScheduleTag(agent, secondDate, scenario, tag);
			var stateholder = SchedulerStateHolderFrom.Fill(scenario, period, new[] { agent }, new IScheduleData[] { assFirstDate, assSecondDate, tagFirstDate, tagSecondDate }, new[] { skillDayFirstDay, skillDaySecondDay });

			Target.Execute(optimizerOriginalPreferences, new NoSchedulingProgress(), stateholder.Schedules[agent].ScheduledDayCollection(period), new OptimizationPreferences(), null);

			stateholder.Schedules[agent].ScheduledDayCollection(period).All(x => x.ScheduleTag().Equals(tag))
				.Should().Be.True();
		}

		[Test]
		public void ShouldKeepTagWhenReplacingShiftAndRollback()
		{
			var firstDate = new DateOnly(2017, 1, 22);
			var secondDate = firstDate.AddDays(1);
			var shiftCategoryBefore = new ShiftCategory("Before").WithId();
			var shiftCategoryAfter = new ShiftCategory("After").WithId();
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var nightRest = TimeSpan.FromHours(11);
			var contract = new Contract("_") { WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(10), TimeSpan.FromHours(83), nightRest, TimeSpan.FromHours(16)) };
			var skill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).IsOpen();
			var skillDayFirstDay = skill.CreateSkillDayWithDemand(scenario, firstDate, 1); //should try with this one first
			var skillDaySecondDay = skill.CreateSkillDayWithDemand(scenario, secondDate, 10);
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(14, 0, 14, 0, 15), new TimePeriodWithSegment(22, 0, 22, 0, 15), shiftCategoryAfter));
			var tag = new ScheduleTag();
			var optimizerOriginalPreferences = createOptimizerOriginalPreferencesTeamSingleAgent();
			optimizerOriginalPreferences.SchedulingOptions.TagToUseOnScheduling = tag;
			var agent = new Person().WithSchedulePeriodOneWeek(firstDate).WithPersonPeriod(ruleSet, contract, skill).InTimeZone(TimeZoneInfo.Utc);
			agent.SchedulePeriod(firstDate).AddShiftCategoryLimitation(new ShiftCategoryLimitation(shiftCategoryBefore) { MaxNumberOf = 1 });
			var period = new DateOnlyPeriod(firstDate, secondDate);
			var assFirstDate = new PersonAssignment(agent, scenario, firstDate).ShiftCategory(shiftCategoryBefore).WithLayer(activity, new TimePeriod(6, 14));
			var assSecondDate = new PersonAssignment(agent, scenario, secondDate).ShiftCategory(shiftCategoryBefore).WithLayer(activity, new TimePeriod(6, 14));
			var tagFirstDate = new AgentDayScheduleTag(agent, firstDate, scenario, tag);
			var tagSecondDate = new AgentDayScheduleTag(agent, secondDate, scenario, tag);
			var stateholder = SchedulerStateHolderFrom.Fill(scenario, period, new[] { agent }, new IScheduleData[] { assFirstDate, assSecondDate, tagFirstDate, tagSecondDate }, new[] { skillDayFirstDay, skillDaySecondDay });

			Target.Execute(optimizerOriginalPreferences, new NoSchedulingProgress(), stateholder.Schedules[agent].ScheduledDayCollection(period), new OptimizationPreferences(), null);

			stateholder.Schedules[agent].ScheduledDayCollection(period).All(x => x.ScheduleTag().Equals(tag))
				.Should().Be.True();
		}

		[Test]
		public void ShouldNotCrashOnTeamMemberNotInSelection()
		{
			var team = new Team { Site = new Site("_") }.WithDescription(new Description("_"));
			GroupScheduleGroupPageDataProvider.SetBusinessUnit_UseFromTestOnly(BusinessUnitFactory.CreateBusinessUnitAndAppend(team));
			var date = new DateOnly(2017, 1, 22);
			var period = new DateOnlyPeriod(date, date);
			var shiftCategory = new ShiftCategory("Before").WithId();
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var skill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).IsOpen();
			var skillDay = skill.CreateSkillDayWithDemand(scenario, date, 2);
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(14, 0, 14, 0, 15), new TimePeriodWithSegment(22, 0, 22, 0, 15), shiftCategory));
			var firstTeamMember = new Person().WithName(new Name("A", "A")).WithSchedulePeriodOneWeek(date).WithPersonPeriod(ruleSet, team, skill).InTimeZone(TimeZoneInfo.Utc);
			var secondTeamMember = new Person().WithName(new Name("B", "B")).WithSchedulePeriodOneWeek(date).WithPersonPeriod(ruleSet, team, skill).InTimeZone(TimeZoneInfo.Utc);
			firstTeamMember.SchedulePeriod(date).AddShiftCategoryLimitation(new ShiftCategoryLimitation(shiftCategory) { MaxNumberOf = 0 });
			secondTeamMember.SchedulePeriod(date).AddShiftCategoryLimitation(new ShiftCategoryLimitation(shiftCategory) { MaxNumberOf = 0 });
			var firstTeamMemberAss = new PersonAssignment(firstTeamMember, scenario, date).ShiftCategory(shiftCategory).WithLayer(activity, new TimePeriod(6, 14));
			var secondTeamMemberAss = new PersonAssignment(secondTeamMember, scenario, date).ShiftCategory(shiftCategory).WithLayer(activity, new TimePeriod(6, 14));
			var stateholder = SchedulerStateHolderFrom.Fill(scenario, period, new[] { firstTeamMember, secondTeamMember }, new[] { firstTeamMemberAss, secondTeamMemberAss }, new[] { skillDay });
			var optimizerOriginalPreferences = new OptimizerOriginalPreferences
			{
				SchedulingOptions =
				{
					GroupOnGroupPageForTeamBlockPer = new GroupPageLight("_", GroupPageType.Hierarchy),
					UseTeam = true,
					UseShiftCategoryLimitations = true,
					TeamSameShiftCategory = true
				}
			};

			Assert.DoesNotThrow(() =>
			{
				Target.Execute(optimizerOriginalPreferences, 
					new NoSchedulingProgress(),
					stateholder.Schedules.SchedulesForPeriod(period, secondTeamMember), 
					new OptimizationPreferences(),
					null);
			});
		}

		[Test]
		public void ShouldReplaceShiftBlock()
		{
			var date = new DateOnly(2017, 1, 22);
			var period = new DateOnlyPeriod(date, date);
			var shiftCategoryBefore = new ShiftCategory("Before").WithId();
			var shiftCategoryAfter = new ShiftCategory("After").WithId();
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var skill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).IsOpen();
			var skillDay = skill.CreateSkillDayWithDemand(scenario, date, 1);
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(14, 0, 14, 0, 15), new TimePeriodWithSegment(22, 0, 22, 0, 15), shiftCategoryAfter));
			var agent = new Person().WithSchedulePeriodOneDay(date).WithPersonPeriod(ruleSet, skill).InTimeZone(TimeZoneInfo.Utc);
			agent.SchedulePeriod(date).AddShiftCategoryLimitation(new ShiftCategoryLimitation(shiftCategoryBefore) { MaxNumberOf = 0 });
			var agentAss = new PersonAssignment(agent, scenario, date).ShiftCategory(shiftCategoryBefore).WithLayer(activity, new TimePeriod(6, 14));
			var stateholder = SchedulerStateHolderFrom.Fill(scenario, period, new[] { agent }, new[] { agentAss}, new[] { skillDay});
			var optimizerOriginalPreferences = new OptimizerOriginalPreferences
			{
				SchedulingOptions =
				{
					GroupOnGroupPageForTeamBlockPer = new GroupPageLight("_", GroupPageType.SingleAgent),
					UseBlock = true,
					BlockFinderTypeForAdvanceScheduling = BlockFinderType.SchedulePeriod,
					UseShiftCategoryLimitations = true,
					BlockSameShiftCategory = true
				}
			};

			Target.Execute(optimizerOriginalPreferences, new NoSchedulingProgress(), stateholder.Schedules.SchedulesForPeriod(period, agent), new OptimizationPreferences(), null);
	
			stateholder.Schedules.SchedulesForDay(date).All(x => x.PersonAssignment().ShiftCategory.Equals(shiftCategoryAfter)).Should().Be.True();
		}

		[Test]
		public void ShouldReplaceShiftBlockMultipleDays()
		{
			var date = new DateOnly(2017, 1, 22);
			var period = new DateOnlyPeriod(date, date.AddDays(1));
			var shiftCategoryBefore = new ShiftCategory("Before").WithId();
			var shiftCategoryAfter = new ShiftCategory("After").WithId();
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var skill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).IsOpen();
			var skillDays = skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, date, 1, 1);
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(14, 0, 14, 0, 15), new TimePeriodWithSegment(22, 0, 22, 0, 15), shiftCategoryAfter));
			var agent = new Person().WithSchedulePeriodTwoDays(date).WithPersonPeriod(ruleSet, skill).InTimeZone(TimeZoneInfo.Utc);
			agent.SchedulePeriod(date).AddShiftCategoryLimitation(new ShiftCategoryLimitation(shiftCategoryBefore) { MaxNumberOf = 1 });
			var stateholder = SchedulerStateHolderFrom.Fill(scenario, period, new[] { agent }, new[]
			{
					new PersonAssignment(agent, scenario, date).ShiftCategory(shiftCategoryBefore).WithLayer(activity, new TimePeriod(6, 14)),
					new PersonAssignment(agent, scenario, date.AddDays(1)).ShiftCategory(shiftCategoryBefore).WithLayer(activity, new TimePeriod(6, 14))
			}, skillDays);
			var optimizerOriginalPreferences = new OptimizerOriginalPreferences
			{
				SchedulingOptions =
				{
					GroupOnGroupPageForTeamBlockPer = new GroupPageLight("_", GroupPageType.SingleAgent),
					UseBlock = true,
					BlockFinderTypeForAdvanceScheduling = BlockFinderType.SchedulePeriod,
					UseShiftCategoryLimitations = true,
					BlockSameShiftCategory = true,
					AllowBreakContractTime = true
				}
			};

			Target.Execute(optimizerOriginalPreferences, new NoSchedulingProgress(), stateholder.Schedules.SchedulesForPeriod(period, agent), new OptimizationPreferences(), null);

			stateholder.Schedules[agent].ScheduledDayCollection(period).All(x => x.PersonAssignment().ShiftCategory.Equals(shiftCategoryAfter))
				.Should().Be.True();
		}

		[Test]
		public void ShouldReportProgress()
		{
			var date = new DateOnly(2017, 1, 22);
			var shiftCategoryBefore = new ShiftCategory("Before").WithId();
			var shiftCategoryAfter = new ShiftCategory("After").WithId();
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var nightRest = TimeSpan.FromHours(11);
			var contract = new Contract("_") { WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(10), TimeSpan.FromHours(83), nightRest, TimeSpan.FromHours(16)) };
			var skill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).IsOpen();
			var skillDayFirstDay = skill.CreateSkillDayWithDemand(scenario, date, 1); 
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(14, 0, 14, 0, 15), new TimePeriodWithSegment(22, 0, 22, 0, 15), shiftCategoryAfter));
			var agent = new Person().WithSchedulePeriodOneWeek(date).WithPersonPeriod(ruleSet, contract, skill).InTimeZone(TimeZoneInfo.Utc);
			agent.SchedulePeriod(date).AddShiftCategoryLimitation(new ShiftCategoryLimitation(shiftCategoryBefore) { MaxNumberOf = 1 });
			var assA = new PersonAssignment(agent, scenario, date).ShiftCategory(shiftCategoryBefore).WithLayer(activity, new TimePeriod(6, 14));
			var stateholder = SchedulerStateHolderFrom.Fill(scenario, date.ToDateOnlyPeriod(), new[] { agent }, new[] { assA }, new[] { skillDayFirstDay });
			var schedulingProgress = new TrackSchedulingProgress<TeleoptiProgressChangeMessage>();

			Target.Execute(createOptimizerOriginalPreferencesTeamSingleAgent(), schedulingProgress, new[] { stateholder.Schedules[agent].ScheduledDay(date) }, new OptimizationPreferences(), null);

			schedulingProgress.ReportedProgress.Select(x => x.Message)
				.Should().Contain(Resources.TryingToResolveShiftCategoryLimitationsDotDotDot);
		}

		private static OptimizerOriginalPreferences createOptimizerOriginalPreferencesTeamSingleAgent()
		{
			var optimizerOriginalPreferences = new OptimizerOriginalPreferences
			{
				SchedulingOptions =
				{
					GroupOnGroupPageForTeamBlockPer = new GroupPageLight("not interesting", GroupPageType.SingleAgent),
					UseTeam = true,
					UseShiftCategoryLimitations = true
				}
			};
			return optimizerOriginalPreferences;
		}
	}
}