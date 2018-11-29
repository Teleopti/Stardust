using System;
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
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Scheduling;
using Teleopti.Ccc.UserTexts;


namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.Scheduling
{
	[DomainTest]
	[UseIocForFatClient]
	public class SchedulingShiftCategoryLimitationDesktopTest : SchedulingScenario
	{
		public DesktopScheduling Target;
		public Func<ISchedulerStateHolder> SchedulerStateHolderFrom;
		public FakeBusinessUnitRepository BusinessUnitRepository;

		[TestCase(1, true)]
		[TestCase(0, false)]
		public void ShouldNotBreakShiftCategoryLimitationOf0WhenMultipleIslandsExist(int maxNumberOfShiftCategories, bool agentShouldBeScheduled)
		{
			var date = new DateOnly(2017, 1, 22);
			var shiftCat = new ShiftCategory("_").WithId();
			var scenario = new Scenario();
			var activity = new Activity();
			var skill1 = new Skill("1").For(activity).InTimeZone(TimeZoneInfo.Utc).IsOpen().WithId();
			var skill2 = new Skill("2").For(activity).InTimeZone(TimeZoneInfo.Utc).IsOpen().WithId();
			var skillDay1 = skill1.CreateSkillDayWithDemand(scenario, date, 1);
			var skillDay2 = skill2.CreateSkillDayWithDemand(scenario, date, 1);
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCat));
			var agent1 = new Person().WithSchedulePeriodOneDay(date).WithPersonPeriod(ruleSet, skill1).InTimeZone(TimeZoneInfo.Utc).WithId();
			agent1.SchedulePeriod(date).AddShiftCategoryLimitation(new ShiftCategoryLimitation(shiftCat) { MaxNumberOf = maxNumberOfShiftCategories });
			var agent2 = new Person().WithSchedulePeriodOneDay(date).WithPersonPeriod(ruleSet, skill2).InTimeZone(TimeZoneInfo.Utc).WithId();
			agent2.SchedulePeriod(date).AddShiftCategoryLimitation(new ShiftCategoryLimitation(shiftCat) { MaxNumberOf = maxNumberOfShiftCategories });
			var stateholder = SchedulerStateHolderFrom.Fill(scenario, date, new[] {agent1, agent2}, new[] {skillDay1, skillDay2});
			var schedulingOptions = new SchedulingOptions
			{ 
				UseShiftCategoryLimitations = true
			};

			Target.Execute(new NoSchedulingCallback(), schedulingOptions, new NoSchedulingProgress(), new[]{ agent1, agent2 }, date.ToDateOnlyPeriod());

			stateholder.Schedules[agent1].ScheduledDay(date).IsScheduled().Should().Be.EqualTo(agentShouldBeScheduled);
			stateholder.Schedules[agent2].ScheduledDay(date).IsScheduled().Should().Be.EqualTo(agentShouldBeScheduled);
		}
		
		[Test]
		public void ShouldRespectShiftCategoryLimitationWhenUsingTeamAndBlock()
		{
			var team = new Team { Site = new Site("_") }.WithDescription(new Description("_"));
			BusinessUnitRepository.Has(BusinessUnitFactory.CreateBusinessUnitAndAppend(team).WithId(ServiceLocatorForEntity.CurrentBusinessUnit.Current().Id.Value));
			var date = new DateOnly(2017, 1, 22);
			var period = DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1);
			var shiftCat1 = new ShiftCategory("1").WithId();
			var shiftCat2 = new ShiftCategory("2").WithId();
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var skill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).IsOpen().WithId();
			var skillDays = skill.CreateSkillDayWithDemand(scenario, period, TimeSpan.FromHours(1));
			var ruleSet1 = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCat1));
			var ruleSet2 = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCat2));
			var ruleSetBag = new RuleSetBag(ruleSet1);
			ruleSetBag.AddRuleSet(ruleSet2);
			var agent1 = new Person().WithSchedulePeriodOneWeek(date).WithPersonPeriod(ruleSetBag, new ContractWithMaximumTolerance(), team, skill).InTimeZone(TimeZoneInfo.Utc).WithId();
			var agent2 = new Person().WithSchedulePeriodOneWeek(date).WithPersonPeriod(ruleSetBag, new ContractWithMaximumTolerance(), team, skill).InTimeZone(TimeZoneInfo.Utc).WithId();
			agent1.SchedulePeriod(date).AddShiftCategoryLimitation(new ShiftCategoryLimitation(shiftCat1) { MaxNumberOf = 0 });
			agent2.SchedulePeriod(date).AddShiftCategoryLimitation(new ShiftCategoryLimitation(shiftCat2) { MaxNumberOf = 0 });
			var stateholder = SchedulerStateHolderFrom.Fill(scenario, period,  new[] { agent1, agent2 }, 
					new []
					{
						new PersonAssignment(agent1, scenario, date).WithDayOff(),
						new PersonAssignment(agent2, scenario, date).WithDayOff(),
						new PersonAssignment(agent1, scenario, date.AddDays(2)).WithDayOff(),
						new PersonAssignment(agent2, scenario, date.AddDays(2)).WithDayOff()
					},
					skillDays
			);
			var schedulingOptions = new SchedulingOptions
			{ 
					GroupOnGroupPageForTeamBlockPer = new GroupPageLight("_", GroupPageType.Hierarchy),
					UseTeam = true,
					UseBlock = true,
					UseShiftCategoryLimitations = true,
					BlockFinderTypeForAdvanceScheduling = BlockFinderType.BetweenDayOff,
					BlockSameShiftCategory = true,
					TeamSameShiftCategory = true
			};

			Target.Execute(new NoSchedulingCallback(), schedulingOptions, new NoSchedulingProgress(), new[]{agent1, agent2}, period);

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
			BusinessUnitRepository.Has(BusinessUnitFactory.CreateBusinessUnitAndAppend(team).WithId(ServiceLocatorForEntity.CurrentBusinessUnit.Current().Id.Value));
			var date = new DateOnly(2017, 1, 22);
			var period = DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1);
			var shiftCat1 = new ShiftCategory("1").WithId();
			var shiftCat2 = new ShiftCategory("2").WithId();
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var skill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).IsOpen().WithId();
			var skillDays = skill.CreateSkillDayWithDemand(scenario, period, TimeSpan.FromHours(1));
			var ruleSet1 = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCat1));
			var ruleSet2 = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCat2));
			var ruleSetBag = new RuleSetBag(ruleSet1);
			ruleSetBag.AddRuleSet(ruleSet2);
			var agent1 = new Person().WithSchedulePeriodOneWeek(date).WithPersonPeriod(ruleSetBag, new ContractWithMaximumTolerance(), team, skill).InTimeZone(TimeZoneInfo.Utc).WithId();
			var agent2 = new Person().WithSchedulePeriodOneWeek(date).WithPersonPeriod(ruleSetBag, new ContractWithMaximumTolerance(), team, skill).InTimeZone(TimeZoneInfo.Utc).WithId();
			agent1.SchedulePeriod(date).AddShiftCategoryLimitation(new ShiftCategoryLimitation(shiftCat1) { MaxNumberOf = 1 });
			agent1.SchedulePeriod(date).AddShiftCategoryLimitation(new ShiftCategoryLimitation(shiftCat2) { MaxNumberOf = 1 });
			agent2.SchedulePeriod(date).AddShiftCategoryLimitation(new ShiftCategoryLimitation(shiftCat1) { MaxNumberOf = 1 });
			agent2.SchedulePeriod(date).AddShiftCategoryLimitation(new ShiftCategoryLimitation(shiftCat2) { MaxNumberOf = 1 });
			var stateholder = SchedulerStateHolderFrom.Fill(scenario, period, new[] { agent1, agent2 },
					new[]
					{
						new PersonAssignment(agent1, scenario, date).WithDayOff(),
						new PersonAssignment(agent2, scenario, date).WithDayOff(),
						new PersonAssignment(agent1, scenario, date.AddDays(3)).WithDayOff(),
						new PersonAssignment(agent2, scenario, date.AddDays(3)).WithDayOff()
					},
					skillDays
			);
			var schedulingOptions = new SchedulingOptions
			{ 
				GroupOnGroupPageForTeamBlockPer = new GroupPageLight("_", GroupPageType.Hierarchy),
				UseTeam = true,
				UseBlock = true,
				UseShiftCategoryLimitations = true,
				BlockFinderTypeForAdvanceScheduling = BlockFinderType.BetweenDayOff,
				BlockSameShiftCategory = true,
				TeamSameShiftCategory = true
			};

			var blockPeriod = new DateOnlyPeriod(date, date.AddDays(3));
			Target.Execute(new NoSchedulingCallback(), schedulingOptions, new NoSchedulingProgress(), new[] { agent1, agent2 }, blockPeriod);

			stateholder.Schedules.SchedulesForPeriod(period, agent1, agent2)
				.Count(x => x.PersonAssignment(true).MainActivities().Any())
				.Should().Be.LessThanOrEqualTo(2);
		}

		[Test]
		public void ShouldNotTouchTeamMembersNotInSelectionWhenUsingTeamAndBlock()
		{
			var team = new Team { Site = new Site("_") }.WithDescription(new Description("_"));
			BusinessUnitRepository.Has(BusinessUnitFactory.CreateBusinessUnitAndAppend(team).WithId(ServiceLocatorForEntity.CurrentBusinessUnit.Current().Id.Value));
			var date = new DateOnly(2017, 1, 22);
			var period = new DateOnlyPeriod(date, date.AddDays(2));
			var shiftCategoryBefore = new ShiftCategory("Before").WithId();
			var shiftCategoryAfter = new ShiftCategory("After").WithId();
			var scenario = new Scenario("_");
			var activity = new Activity();
			var skill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).IsOpen().WithId();
			var skillDay = skill.CreateSkillDayWithDemand(scenario, date.AddDays(1), 2);
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(14, 0, 14, 0, 15), new TimePeriodWithSegment(22, 0, 22, 0, 15), shiftCategoryAfter));
			var selectedAgent = new Person().WithSchedulePeriodOneWeek(date, 2).WithPersonPeriod(ruleSet, team, skill).InTimeZone(TimeZoneInfo.Utc).WithId();
			var agentNotInSelection = new Person().WithSchedulePeriodOneWeek(date, 2).WithPersonPeriod(ruleSet, team, skill).InTimeZone(TimeZoneInfo.Utc).WithId();
			selectedAgent.SchedulePeriod(date).AddShiftCategoryLimitation(new ShiftCategoryLimitation(shiftCategoryBefore) { MaxNumberOf = 0 });
			agentNotInSelection.SchedulePeriod(date).AddShiftCategoryLimitation(new ShiftCategoryLimitation(shiftCategoryBefore) { MaxNumberOf = 0 });
			var stateholder = SchedulerStateHolderFrom.Fill(scenario, period, new[] { selectedAgent, agentNotInSelection}, new[]
			{
				new PersonAssignment(selectedAgent, scenario, date).WithDayOff(),
				new PersonAssignment(selectedAgent, scenario, date.AddDays(1)).ShiftCategory(shiftCategoryBefore).WithLayer(activity, new TimePeriod(6, 14)),
				new PersonAssignment(selectedAgent, scenario, date.AddDays(2)).WithDayOff(),
				new PersonAssignment(agentNotInSelection, scenario, date).WithDayOff(),
				new PersonAssignment(agentNotInSelection, scenario, date.AddDays(2)).WithDayOff()
			}, skillDay);
			var schedulingOptions = new SchedulingOptions
			{ 
				GroupOnGroupPageForTeamBlockPer = new GroupPageLight("_", GroupPageType.Hierarchy),
				UseTeam = true,
				UseBlock = true,
				UseShiftCategoryLimitations = true,
				BlockFinderTypeForAdvanceScheduling = BlockFinderType.BetweenDayOff,
				BlockSameShiftCategory = true,
				TeamSameShiftCategory = true,
				AllowBreakContractTime = true
			};

			Target.Execute(new NoSchedulingCallback(), schedulingOptions, new NoSchedulingProgress(), new[] { selectedAgent }, period);

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
			var skill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).IsOpen().WithId();
			var skillDayFirstDay = skill.CreateSkillDayWithDemand(scenario, firstDate, 1); //should try with this one first
			var skillDaySecondDay = skill.CreateSkillDayWithDemand(scenario, secondDate, 10); 
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(14, 0, 14, 0, 15), new TimePeriodWithSegment(22, 0, 22, 0, 15), shiftCategoryAfter));
			var agent = new Person().WithSchedulePeriodOneWeek(firstDate).WithPersonPeriod(ruleSet, contract, skill).InTimeZone(TimeZoneInfo.Utc).WithId();
			agent.SchedulePeriod(firstDate).AddShiftCategoryLimitation(new ShiftCategoryLimitation(shiftCategoryBefore) { MaxNumberOf = 1 });
			var period = new DateOnlyPeriod(firstDate, secondDate);
			var assA = new PersonAssignment(agent, scenario, firstDate).ShiftCategory(shiftCategoryBefore).WithLayer(activity, new TimePeriod(6, 14));
			var assB = new PersonAssignment(agent, scenario, secondDate).ShiftCategory(shiftCategoryBefore).WithLayer(activity, new TimePeriod(6, 14));
			var stateholder = SchedulerStateHolderFrom.Fill(scenario, period, new[] { agent }, new[] { assA, assB }, new[] { skillDayFirstDay, skillDaySecondDay });

			Target.Execute(new NoSchedulingCallback(), createSchedulingOptionsTeamSingleAgent(), new NoSchedulingProgress(), new[] { agent }, period);

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
			var skill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).IsOpen().WithId();
			var skillDay = skill.CreateSkillDayWithDemand(scenario, date, 1);
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(14, 0, 14, 0, 15), new TimePeriodWithSegment(22, 0, 22, 0, 15), shiftCategory));
			var agent = new Person().WithSchedulePeriodOneWeek(date).WithPersonPeriod(ruleSet, contract, skill).InTimeZone(TimeZoneInfo.Utc).WithId();
			agent.SchedulePeriod(date).AddShiftCategoryLimitation(new ShiftCategoryLimitation(shiftCategory) { MaxNumberOf = 1 });
			var period = new DateOnlyPeriod(date, date);
			var assA = new PersonAssignment(agent, scenario, date).ShiftCategory(shiftCategory).WithLayer(activity, new TimePeriod(6, 14));
			var assB = new PersonAssignment(agent, scenario, date.AddDays(1)).ShiftCategory(shiftCategory).WithLayer(activity, new TimePeriod(6, 14));
			var stateholder = SchedulerStateHolderFrom.Fill(scenario, period, new[] { agent }, new[] { assA, assB}, new[] { skillDay});

			Target.Execute(new NoSchedulingCallback(), createSchedulingOptionsTeamSingleAgent(), new NoSchedulingProgress(), new[] { agent }, period);

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
			var skill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).IsOpen().WithId();
			var skillDayFirstDay = skill.CreateSkillDayWithDemand(scenario, firstDate, 10); 
			var skillDaySecondDay = skill.CreateSkillDayWithDemand(scenario, secondDate, 1); //should try with this one first
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(14, 0, 14, 0, 15), new TimePeriodWithSegment(22, 0, 22, 0, 15), shiftCategoryAfter));
			var agent = new Person().WithSchedulePeriodOneWeek(firstDate).WithPersonPeriod(ruleSet, contract, skill).InTimeZone(TimeZoneInfo.Utc).WithId();
			agent.SchedulePeriod(firstDate).AddShiftCategoryLimitation(new ShiftCategoryLimitation(shiftCategoryBefore) { MaxNumberOf = 1 });
			var period = new DateOnlyPeriod(firstDate, secondDate);
			var assA = new PersonAssignment(agent, scenario, firstDate).ShiftCategory(shiftCategoryBefore).WithLayer(activity, new TimePeriod(6, 14));
			var assB = new PersonAssignment(agent, scenario, secondDate).ShiftCategory(shiftCategoryBefore).WithLayer(activity, new TimePeriod(6, 14));
			var stateholder = SchedulerStateHolderFrom.Fill(scenario, period, new[] { agent }, new[] { assA, assB }, new[] { skillDayFirstDay, skillDaySecondDay });

			Target.Execute(new NoSchedulingCallback(), createSchedulingOptionsTeamSingleAgent(), new NoSchedulingProgress(), new[] { agent }, period);

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
			var skill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).IsOpen().WithId();
			var skillDayFirstDay = skill.CreateSkillDayWithDemand(scenario, firstDate, 2); //should try with this one first, but can't because of night rest
			var skillDaySecondDay = skill.CreateSkillDayWithDemand(scenario, secondDate, 10);
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(14, 0, 14, 0, 15), new TimePeriodWithSegment(22, 0, 22, 0, 15), shiftCategoryAfter));
			var firstAgent = new Person().WithName(new Name("first", "first")).WithSchedulePeriodOneWeek(firstDate).WithPersonPeriod(ruleSet, contract, skill).InTimeZone(TimeZoneInfo.Utc).WithId();
			var secondAgent = new Person().WithName(new Name("second", "second")).WithSchedulePeriodOneWeek(firstDate).WithPersonPeriod(ruleSet, contract, skill).InTimeZone(TimeZoneInfo.Utc).WithId();
			firstAgent.SchedulePeriod(firstDate).AddShiftCategoryLimitation(new ShiftCategoryLimitation(shiftCategoryBefore) { MaxNumberOf = 1 });
			secondAgent.SchedulePeriod(firstDate).AddShiftCategoryLimitation(new ShiftCategoryLimitation(shiftCategoryBefore) { MaxNumberOf = 1 });
			var firstAgentAssA = new PersonAssignment(firstAgent, scenario, firstDate).ShiftCategory(shiftCategoryBefore).WithLayer(activity, new TimePeriod(6, 14));
			var firstAgentAssB = new PersonAssignment(firstAgent, scenario, secondDate).ShiftCategory(shiftCategoryBefore).WithLayer(activity, new TimePeriod(6, 14));
			var secondAgentAssA = new PersonAssignment(secondAgent, scenario, firstDate).ShiftCategory(shiftCategoryBefore).WithLayer(activity, new TimePeriod(6, 14));
			var secondAgentAssB = new PersonAssignment(secondAgent, scenario, secondDate).ShiftCategory(shiftCategoryBefore).WithLayer(activity, new TimePeriod(6, 14));
			var stateholder = SchedulerStateHolderFrom.Fill(scenario, period, new[] { firstAgent, secondAgent }, new[] { firstAgentAssA, firstAgentAssB, secondAgentAssA, secondAgentAssB }, new[] { skillDayFirstDay, skillDaySecondDay });

			Target.Execute(new NoSchedulingCallback(), createSchedulingOptionsTeamSingleAgent(), new NoSchedulingProgress(), new[] { firstAgent, secondAgent }, period);

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
			var skill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).IsOpen().WithId();
			var skillDayFirstDay = skill.CreateSkillDayWithDemand(scenario, firstDate, 1);
			var skillDaySecondDay = skill.CreateSkillDayWithDemand(scenario, secondDate, 10);
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(14, 0, 14, 0, 15), new TimePeriodWithSegment(22, 0, 22, 0, 15), shiftCategoryAfter));
			var tag = new ScheduleTag();
			var schedulingOptions = createSchedulingOptionsTeamSingleAgent();
			schedulingOptions.TagToUseOnScheduling = tag;
			var agent = new Person().WithSchedulePeriodOneWeek(firstDate).WithPersonPeriod(ruleSet, contract, skill).InTimeZone(TimeZoneInfo.Utc).WithId();
			agent.SchedulePeriod(firstDate).AddShiftCategoryLimitation(new ShiftCategoryLimitation(shiftCategoryBefore) { MaxNumberOf = 1 });
			var period = new DateOnlyPeriod(firstDate, secondDate);
			var assFirstDate = new PersonAssignment(agent, scenario, firstDate).ShiftCategory(shiftCategoryBefore).WithLayer(activity, new TimePeriod(6, 14));
			var assSecondDate = new PersonAssignment(agent, scenario, secondDate).ShiftCategory(shiftCategoryBefore).WithLayer(activity, new TimePeriod(6, 14));
			var stateholder = SchedulerStateHolderFrom.Fill(scenario, period, new[] { agent }, new IScheduleData[] { assFirstDate, assSecondDate }, new[] { skillDayFirstDay, skillDaySecondDay });

			Target.Execute(new NoSchedulingCallback(), schedulingOptions, new NoSchedulingProgress(), new[] { agent }, period);

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
			var skill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).IsOpen().WithId();
			var skillDayFirstDay = skill.CreateSkillDayWithDemand(scenario, firstDate, 1);
			var skillDaySecondDay = skill.CreateSkillDayWithDemand(scenario, secondDate, 10);
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(14, 0, 14, 0, 15), new TimePeriodWithSegment(22, 0, 22, 0, 15), shiftCategoryAfter));
			var tag = new ScheduleTag();
			var schedulingOptions = createSchedulingOptionsTeamSingleAgent();
			schedulingOptions.TagToUseOnScheduling = tag;
			var agent = new Person().WithSchedulePeriodOneWeek(firstDate).WithPersonPeriod(ruleSet, contract, skill).InTimeZone(TimeZoneInfo.Utc).WithId();
			agent.SchedulePeriod(firstDate).AddShiftCategoryLimitation(new ShiftCategoryLimitation(shiftCategoryBefore) { MaxNumberOf = 1 });
			var period = new DateOnlyPeriod(firstDate, secondDate);
			var assFirstDate = new PersonAssignment(agent, scenario, firstDate).ShiftCategory(shiftCategoryBefore).WithLayer(activity, new TimePeriod(6, 14));
			var assSecondDate = new PersonAssignment(agent, scenario, secondDate).ShiftCategory(shiftCategoryBefore).WithLayer(activity, new TimePeriod(6, 14));
			var tagFirstDate = new AgentDayScheduleTag(agent, firstDate, scenario, tag);
			var tagSecondDate = new AgentDayScheduleTag(agent, secondDate, scenario, tag);
			var stateholder = SchedulerStateHolderFrom.Fill(scenario, period, new[] { agent }, new IScheduleData[] { assFirstDate, assSecondDate, tagFirstDate, tagSecondDate }, new[] { skillDayFirstDay, skillDaySecondDay });

			Target.Execute(new NoSchedulingCallback(), schedulingOptions, new NoSchedulingProgress(), new[] { agent }, period);

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
			var skill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).IsOpen().WithId();
			var skillDayFirstDay = skill.CreateSkillDayWithDemand(scenario, firstDate, 1); //should try with this one first
			var skillDaySecondDay = skill.CreateSkillDayWithDemand(scenario, secondDate, 10);
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(14, 0, 14, 0, 15), new TimePeriodWithSegment(22, 0, 22, 0, 15), shiftCategoryAfter));
			var tag = new ScheduleTag();
			var schedulingOptions = createSchedulingOptionsTeamSingleAgent();
			schedulingOptions.TagToUseOnScheduling = tag;
			var agent = new Person().WithSchedulePeriodOneWeek(firstDate).WithPersonPeriod(ruleSet, contract, skill).InTimeZone(TimeZoneInfo.Utc).WithId();
			agent.SchedulePeriod(firstDate).AddShiftCategoryLimitation(new ShiftCategoryLimitation(shiftCategoryBefore) { MaxNumberOf = 1 });
			var period = new DateOnlyPeriod(firstDate, secondDate);
			var assFirstDate = new PersonAssignment(agent, scenario, firstDate).ShiftCategory(shiftCategoryBefore).WithLayer(activity, new TimePeriod(6, 14));
			var assSecondDate = new PersonAssignment(agent, scenario, secondDate).ShiftCategory(shiftCategoryBefore).WithLayer(activity, new TimePeriod(6, 14));
			var tagFirstDate = new AgentDayScheduleTag(agent, firstDate, scenario, tag);
			var tagSecondDate = new AgentDayScheduleTag(agent, secondDate, scenario, tag);
			var stateholder = SchedulerStateHolderFrom.Fill(scenario, period, new[] { agent }, new IScheduleData[] { assFirstDate, assSecondDate, tagFirstDate, tagSecondDate }, new[] { skillDayFirstDay, skillDaySecondDay });

			Target.Execute(new NoSchedulingCallback(), schedulingOptions, new NoSchedulingProgress(), new[] { agent }, period);

			stateholder.Schedules[agent].ScheduledDayCollection(period).All(x => x.ScheduleTag().Equals(tag))
				.Should().Be.True();
		}

		[Test]
		public void ShouldNotCrashOnTeamMemberNotInSelection()
		{
			var team = new Team { Site = new Site("_") }.WithDescription(new Description("_"));
			BusinessUnitRepository.Has(BusinessUnitFactory.CreateBusinessUnitAndAppend(team).WithId(ServiceLocatorForEntity.CurrentBusinessUnit.Current().Id.Value));
			var date = new DateOnly(2017, 1, 22);
			var period = new DateOnlyPeriod(date, date);
			var shiftCategory = new ShiftCategory("Before").WithId();
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var skill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).IsOpen().WithId();
			var skillDay = skill.CreateSkillDayWithDemand(scenario, date, 2);
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(14, 0, 14, 0, 15), new TimePeriodWithSegment(22, 0, 22, 0, 15), shiftCategory));
			var firstTeamMember = new Person().WithName(new Name("A", "A")).WithSchedulePeriodOneWeek(date).WithPersonPeriod(ruleSet, team, skill).InTimeZone(TimeZoneInfo.Utc).WithId();
			var secondTeamMember = new Person().WithName(new Name("B", "B")).WithSchedulePeriodOneWeek(date).WithPersonPeriod(ruleSet, team, skill).InTimeZone(TimeZoneInfo.Utc).WithId();
			firstTeamMember.SchedulePeriod(date).AddShiftCategoryLimitation(new ShiftCategoryLimitation(shiftCategory) { MaxNumberOf = 0 });
			secondTeamMember.SchedulePeriod(date).AddShiftCategoryLimitation(new ShiftCategoryLimitation(shiftCategory) { MaxNumberOf = 0 });
			var firstTeamMemberAss = new PersonAssignment(firstTeamMember, scenario, date).ShiftCategory(shiftCategory).WithLayer(activity, new TimePeriod(6, 14));
			var secondTeamMemberAss = new PersonAssignment(secondTeamMember, scenario, date).ShiftCategory(shiftCategory).WithLayer(activity, new TimePeriod(6, 14));
			SchedulerStateHolderFrom.Fill(scenario, period, new[] { firstTeamMember, secondTeamMember }, new[] { firstTeamMemberAss, secondTeamMemberAss }, new[] { skillDay });
			var schedulingOptions = new SchedulingOptions
			{ 
				GroupOnGroupPageForTeamBlockPer = new GroupPageLight("_", GroupPageType.Hierarchy),
				UseTeam = true,
				UseShiftCategoryLimitations = true,
				TeamSameShiftCategory = true
			};

			Assert.DoesNotThrow(() =>
			{
				Target.Execute(new NoSchedulingCallback(), 
					schedulingOptions,
					new NoSchedulingProgress(),
					new[] {secondTeamMember}, period);
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
			var skill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).IsOpen().WithId().WithId();
			var skillDay = skill.CreateSkillDayWithDemand(scenario, date, 1);
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(14, 0, 14, 0, 15), new TimePeriodWithSegment(22, 0, 22, 0, 15), shiftCategoryAfter));
			var agent = new Person().WithSchedulePeriodOneDay(date).WithPersonPeriod(ruleSet, skill).InTimeZone(TimeZoneInfo.Utc).WithId();
			agent.SchedulePeriod(date).AddShiftCategoryLimitation(new ShiftCategoryLimitation(shiftCategoryBefore) { MaxNumberOf = 0 });
			var agentAss = new PersonAssignment(agent, scenario, date).ShiftCategory(shiftCategoryBefore).WithLayer(activity, new TimePeriod(6, 14));
			var stateholder = SchedulerStateHolderFrom.Fill(scenario, period, new[] { agent }, new[] { agentAss}, new[] { skillDay});
			var schedulingOptions = new SchedulingOptions
			{
				GroupOnGroupPageForTeamBlockPer = new GroupPageLight("_", GroupPageType.SingleAgent),
				UseBlock = true,
				BlockFinderTypeForAdvanceScheduling = BlockFinderType.SchedulePeriod,
				UseShiftCategoryLimitations = true,
				BlockSameShiftCategory = true
			};

			Target.Execute(new NoSchedulingCallback(), schedulingOptions, new NoSchedulingProgress(), new[] { agent }, period);
	
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
			var skill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).IsOpen().WithId();
			var skillDays = skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, date, 1, 1);
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(14, 0, 14, 0, 15), new TimePeriodWithSegment(22, 0, 22, 0, 15), shiftCategoryAfter));
			var agent = new Person().WithSchedulePeriodTwoDays(date).WithPersonPeriod(ruleSet, skill).InTimeZone(TimeZoneInfo.Utc).WithId();
			agent.SchedulePeriod(date).AddShiftCategoryLimitation(new ShiftCategoryLimitation(shiftCategoryBefore) { MaxNumberOf = 1 });
			var stateholder = SchedulerStateHolderFrom.Fill(scenario, period, new[] { agent }, new[]
			{
					new PersonAssignment(agent, scenario, date).ShiftCategory(shiftCategoryBefore).WithLayer(activity, new TimePeriod(6, 14)),
					new PersonAssignment(agent, scenario, date.AddDays(1)).ShiftCategory(shiftCategoryBefore).WithLayer(activity, new TimePeriod(6, 14))
			}, skillDays);
			var schedulingOptions = new SchedulingOptions
			{
				GroupOnGroupPageForTeamBlockPer = new GroupPageLight("_", GroupPageType.SingleAgent),
				UseBlock = true,
				BlockFinderTypeForAdvanceScheduling = BlockFinderType.SchedulePeriod,
				UseShiftCategoryLimitations = true,
				BlockSameShiftCategory = true,
				AllowBreakContractTime = true
			};

			Target.Execute(new NoSchedulingCallback(), schedulingOptions, new NoSchedulingProgress(), new[] { agent }, period);

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
			var skill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).IsOpen().WithId();
			var skillDayFirstDay = skill.CreateSkillDayWithDemand(scenario, date, 1); 
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(14, 0, 14, 0, 15), new TimePeriodWithSegment(22, 0, 22, 0, 15), shiftCategoryAfter));
			var agent = new Person().WithSchedulePeriodOneWeek(date).WithPersonPeriod(ruleSet, contract, skill).InTimeZone(TimeZoneInfo.Utc).WithId();
			agent.SchedulePeriod(date).AddShiftCategoryLimitation(new ShiftCategoryLimitation(shiftCategoryBefore) { MaxNumberOf = 1 });
			var assA = new PersonAssignment(agent, scenario, date).ShiftCategory(shiftCategoryBefore).WithLayer(activity, new TimePeriod(6, 14));
			SchedulerStateHolderFrom.Fill(scenario, date.ToDateOnlyPeriod(), new[] { agent }, new[] { assA }, new[] { skillDayFirstDay });
			var schedulingProgress = new TrackSchedulingProgress<TeleoptiProgressChangeMessage>();
			var schedulingCallback = new SchedulingCallbackForDesktop(schedulingProgress, new SchedulingOptions());

			Target.Execute(schedulingCallback, createSchedulingOptionsTeamSingleAgent(), schedulingProgress, new[] { agent }, date.ToDateOnlyPeriod());

			schedulingProgress.ReportedProgress.Select(x => x.Message)
				.Should().Contain(Resources.TryingToResolveShiftCategoryLimitationsDotDotDot);
		}

		[TestCase(true)]
		[TestCase(false)]
		public void ShouldNotCrashWhenSchedulingHourlyEmployeesWithShiftCategoryLimitations(bool useShiftCategoryLimitations)
		{
			var date = new DateOnly(2017, 1, 22);
			var shiftCat = new ShiftCategory().WithId();
			var scenario = new Scenario();
			var activity = new Activity();
			var skill = new Skill().For(activity).InTimeZone(TimeZoneInfo.Utc).IsOpen().WithId();
			var skillDay = skill.CreateSkillDayWithDemand(scenario, date, 1);
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCat));
			var workTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(0), TimeSpan.FromHours(90), TimeSpan.FromHours(8), TimeSpan.FromHours(0));
			var contractHourly = new Contract("_") { WorkTimeDirective = workTimeDirective, EmploymentType = EmploymentType.HourlyStaff };
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet, contractHourly, skill).WithSchedulePeriodOneWeek(date);
			agent.SchedulePeriod(date).AddShiftCategoryLimitation(new ShiftCategoryLimitation(shiftCat) { MaxNumberOf = 0 });
			SchedulerStateHolderFrom.Fill(scenario, date, new[] { agent }, new[] { skillDay });
			var schedulingOptions = new SchedulingOptions{UseShiftCategoryLimitations = useShiftCategoryLimitations, ScheduleEmploymentType = ScheduleEmploymentType.HourlyStaff};

			Assert.DoesNotThrow(() =>
			{
				Target.Execute(new NoSchedulingCallback(), schedulingOptions, new NoSchedulingProgress(), new[] {agent}, date.ToDateOnlyPeriod());
			});
		}

		private static SchedulingOptions createSchedulingOptionsTeamSingleAgent()
		{
			return new SchedulingOptions
			{
				GroupOnGroupPageForTeamBlockPer = new GroupPageLight("not interesting", GroupPageType.SingleAgent),
				UseTeam = true,
				UseShiftCategoryLimitations = true
			};
		}

		public SchedulingShiftCategoryLimitationDesktopTest(ResourcePlannerTestParameters resourcePlannerTestParameters) : base(resourcePlannerTestParameters)
		{
		}
	}
}