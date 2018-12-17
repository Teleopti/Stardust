using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Scheduling;


namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.DayOffOptimization
{
	/* TODO: some if(individual) asserts here. 
	 * Just here until we make teamblock work the same as for individual
	*/
	[DomainTest]
	[UseIocForFatClient]
	[UseEventPublisher(typeof(SyncInFatClientProcessEventPublisher))]
	public class DayOffOptimizationDesktopTest : DayOffOptimizationScenario
	{
		private readonly ResourcePlannerTestParameters _resourcePlannerTestParameters;
		public Func<ISchedulerStateHolder> SchedulerStateHolder;
		public DayOffOptimizationDesktop Target;
		public Func<IGridlockManager> LockManager;

		[Test]
		public void ShouldMoveDayOffToDayWithLessDemand()
		{
			var firstDay = new DateOnly(2015, 10, 12); //mon
			var period = DateOnlyPeriod.CreateWithNumberOfWeeks(firstDay, 1);
			var activity = new Activity("_");
			var skill = new Skill().WithId().For(activity).IsOpen();
			var scenario = new Scenario("_");
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var team = new Team { Site = new Site("_") };
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet, team, skill).WithSchedulePeriodOneWeek(firstDay);
			agent.SchedulePeriod(firstDay).SetDaysOff(1);
			var skillDays = skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay,
				5,
				1,
				5,
				5,
				5,
				25,
				5);
		
			var asses = Enumerable.Range(0, 7).Select(i => new PersonAssignment(agent, scenario, firstDay.AddDays(i)).ShiftCategory(shiftCategory).WithLayer(activity, new TimePeriod(8, 16))).ToArray();
			asses[5].SetDayOff(new DayOffTemplate()); //saturday
			var stateHolder = SchedulerStateHolder.Fill(scenario, period, new[] {agent}, asses, skillDays);
			var optPrefs = new OptimizationPreferences {General = {ScheduleTag = new ScheduleTag()}};

			Target.Execute(period, new[] { agent }, optPrefs, new FixedDayOffOptimizationPreferenceProvider(new DaysOffPreferences()), new NoOptimizationCallback());

			stateHolder.Schedules[agent].ScheduledDay(firstDay.AddDays(5)).HasDayOff()
				.Should().Be.False();//saturday
			stateHolder.Schedules[agent].ScheduledDay(firstDay.AddDays(1)).HasDayOff()
				.Should().Be.True();//tuesday
		}

		[Test]
		public void ShouldMoveDayOffToDayWithLessDemandAndNotConsiderBrokenMaxSeatOnOtherSite()
		{
			var firstDay = new DateOnly(2015, 10, 12); //mon
			var period = DateOnlyPeriod.CreateWithNumberOfWeeks(firstDay, 1);
			var activity = new Activity("_") {RequiresSeat = true};
			var skill = new Skill().WithId().For(activity).IsOpen();
			var skillMaxSeat = new Skill().For(activity).IsOpen();
			var scenario = new Scenario("_");
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var site = new Site("_");
			var siteMaxSeat = new Site("siteMaxSeat"){MaxSeats = 0};
			var team = new Team { Site = site };
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet, team, skill).WithSchedulePeriodOneWeek(firstDay);
			var teamMaxSeat = new Team { Site = siteMaxSeat };
			var agentMaxSeat = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet, teamMaxSeat, skillMaxSeat).WithSchedulePeriodOneWeek(firstDay);
			agent.SchedulePeriod(firstDay).SetDaysOff(1);
			agentMaxSeat.SchedulePeriod(firstDay).SetDaysOff(1);
			var skillDays = skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay,
				5,
				1,
				5,
				5,
				5,
				25,
				5);

			var skillDaysMaxSeat = skillMaxSeat.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay,
				2,
				2,
				2,
				2,
				2,
				2,
				2);
			var asses = new List<IPersonAssignment>();
			for (var i = 0; i < 7; i++)
			{
				var ass = new PersonAssignment(agent, scenario, firstDay.AddDays(i)).ShiftCategory(shiftCategory).WithLayer(activity, new TimePeriod(8, 16));
				var assMaxSeat = new PersonAssignment(agentMaxSeat, scenario, firstDay.AddDays(i)).ShiftCategory(shiftCategory).WithLayer(activity, new TimePeriod(8, 16));
				asses.Add(ass);
				asses.Add(assMaxSeat);
				if (i != 5) continue;
				ass.SetDayOff(new DayOffTemplate()); //saturday
				assMaxSeat.SetDayOff(new DayOffTemplate()); //saturday
			}

			var stateHolder = SchedulerStateHolder.Fill(scenario, period, new[] { agent }, asses, skillDays.Union(skillDaysMaxSeat));
			var optPrefs = new OptimizationPreferences
			{
				General = {ScheduleTag = new ScheduleTag()},
				Advanced = {UserOptionMaxSeatsFeature = MaxSeatsFeatureOptions.ConsiderMaxSeatsAndDoNotBreak}
			};

			Target.Execute(period, new[] { agent }, optPrefs, new FixedDayOffOptimizationPreferenceProvider(new DaysOffPreferences()), new NoOptimizationCallback());

			stateHolder.Schedules[agent].ScheduledDay(firstDay.AddDays(5)).HasDayOff().Should().Be.False();//saturday
			stateHolder.Schedules[agent].ScheduledDay(firstDay.AddDays(1)).HasDayOff().Should().Be.True();//tuesday
		}


		[TestCase(TeamBlockType.Team)]
		[TestCase(TeamBlockType.Block)]
		[TestCase(TeamBlockType.TeamAndBlock)]
		[TestCase(TeamBlockType.Individual)]
		public void ShouldMoveDayOffToDayWithLessDemand_MarkedBlankDay(TeamBlockType teamBlockType)
		{
			var firstDay = new DateOnly(2015, 10, 12); //mon
			var period = DateOnlyPeriod.CreateWithNumberOfWeeks(firstDay, 1);
			var activity = new Activity("_");
			var skill = new Skill().WithId().For(activity).IsOpen();
			var scenario = new Scenario("_");
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet, skill).WithSchedulePeriodOneWeek(firstDay);
			agent.SchedulePeriod(firstDay).SetDaysOff(1);
			var skillDays = skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay,
				5,
				1,
				5,
				5,
				5,
				25,
				5);
			var asses = Enumerable.Range(0, 7).Select(i => new PersonAssignment(agent, scenario, firstDay.AddDays(i)).ShiftCategory(shiftCategory).WithLayer(activity, new TimePeriod(8, 16))).ToArray();
			asses[5].SetDayOff(new DayOffTemplate());
			asses[0].ClearMainActivities();//blank day
			var stateHolder = SchedulerStateHolder.Fill(scenario, period, new[] { agent }, asses, skillDays);
			var optPrefs = new OptimizationPreferences
			{
				General = { ScheduleTag = new ScheduleTag() },
				Extra = teamBlockType.CreateExtraPreferences()
			};

			Target.Execute(period, new[] { agent }, optPrefs, new FixedDayOffOptimizationPreferenceProvider(new DaysOffPreferences()), new NoOptimizationCallback());

			var wasModified = !stateHolder.Schedules[agent].ScheduledDay(firstDay.AddDays(5)).HasDayOff();
			if (teamBlockType == TeamBlockType.Individual)
			{
				wasModified.Should().Be.False();
			}
			else
			{
				wasModified.Should().Be.True();
			}
		}

		[TestCase(TeamBlockType.Team)]
		[TestCase(TeamBlockType.Block)]
		[TestCase(TeamBlockType.TeamAndBlock)]
		[TestCase(TeamBlockType.Individual)]
		public void ShouldMoveDayOffToDayWithLessDemandPerAgent_MarkedBlankDay(TeamBlockType teamBlockType)
		{
			var firstDay = new DateOnly(2015, 10, 12); //mon
			var period = DateOnlyPeriod.CreateWithNumberOfWeeks(firstDay, 1);
			var activity = new Activity("_");
			var skill = new Skill().WithId().For(activity).IsOpen();
			var scenario = new Scenario("_");
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var agent1 = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet, skill).WithSchedulePeriodOneWeek(firstDay);
			var agent2 = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet, skill).WithSchedulePeriodOneWeek(firstDay);
			agent1.SchedulePeriod(firstDay).SetDaysOff(1);
			agent2.SchedulePeriod(firstDay).SetDaysOff(1);
			var skillDays = skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay,
				5,
				1,
				5,
				5,
				5,
				25,
				5);
			var asses1 = Enumerable.Range(0, 7).Select(i => new PersonAssignment(agent1, scenario, firstDay.AddDays(i)).ShiftCategory(shiftCategory).WithLayer(activity, new TimePeriod(8, 16))).ToArray();
			var asses2 = Enumerable.Range(0, 7).Select(i => new PersonAssignment(agent2, scenario, firstDay.AddDays(i)).ShiftCategory(shiftCategory).WithLayer(activity, new TimePeriod(8, 16))).ToArray();
			asses1[5].SetDayOff(new DayOffTemplate());
			asses2[5].SetDayOff(new DayOffTemplate());
			asses1[0].ClearMainActivities();//bland day
			var stateHolder = SchedulerStateHolder.Fill(scenario, period, new[] { agent1, agent2 }, asses1.Union(asses2), skillDays);
			var optPrefs = new OptimizationPreferences
			{
				General = { ScheduleTag = new ScheduleTag() },
				Extra = teamBlockType.CreateExtraPreferences()
			};

			Target.Execute(period, new[] { agent1, agent2 }, optPrefs, new FixedDayOffOptimizationPreferenceProvider(new DaysOffPreferences()), new NoOptimizationCallback());

			var wasModified1 = !stateHolder.Schedules[agent1].ScheduledDay(firstDay.AddDays(5)).HasDayOff();
			var wasModified2 = !stateHolder.Schedules[agent2].ScheduledDay(firstDay.AddDays(5)).HasDayOff();
			if (teamBlockType == TeamBlockType.Individual)
			{
				wasModified1.Should().Be.False();
				wasModified2.Should().Be.True();
			}
			else
			{
				wasModified1.Should().Be.True();
				wasModified2.Should().Be.True();
			}
		}

		[TestCase(TeamBlockType.Team)]
		[TestCase(TeamBlockType.Block)]
		[TestCase(TeamBlockType.TeamAndBlock)]
		[TestCase(TeamBlockType.Individual)]
		public void ShouldGetBackToLegalState(TeamBlockType teamBlockType)
		{
			var firstDay = new DateOnly(2015, 10, 12); //mon
			var period = DateOnlyPeriod.CreateWithNumberOfWeeks(firstDay, 1);
			var activity = new Activity("_");
			var contract = new ContractWithMaximumTolerance();
			var skill = new Skill().WithId().For(activity).IsOpen();
			var scenario = new Scenario("_");
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet, contract, skill).WithSchedulePeriodOneWeek(firstDay);
			agent.SchedulePeriod(firstDay).SetDaysOff(1);
			var skillDays = skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay,
				5,
				5,
				1,
				1,
				5,
				5,
				5);
			var asses = Enumerable.Range(0, 7).Select(i => new PersonAssignment(agent, scenario, firstDay.AddDays(i)).ShiftCategory(shiftCategory).WithLayer(activity, new TimePeriod(8, 16))).ToArray();
			asses[2].SetDayOff(new DayOffTemplate());
			asses[3].SetDayOff(new DayOffTemplate());
			var stateHolder = SchedulerStateHolder.Fill(scenario, period, new[] { agent }, asses, skillDays);
			var optPrefs = new OptimizationPreferences
			{
				General = { ScheduleTag = new ScheduleTag() },
				Extra = teamBlockType.CreateExtraPreferences()
			};
			var dayOffsPreferences = new DaysOffPreferences
			{
				UseFullWeekendsOff = true,
				FullWeekendsOffValue = new MinMax<int>(1, 1)
			};

			Target.Execute(period, new[] { agent }, optPrefs, new FixedDayOffOptimizationPreferenceProvider(dayOffsPreferences), new NoOptimizationCallback());

			var wasModified1 = stateHolder.Schedules[agent].ScheduledDay(firstDay.AddDays(5)).HasDayOff();
			var wasModified2 = stateHolder.Schedules[agent].ScheduledDay(firstDay.AddDays(6)).HasDayOff();
			if (teamBlockType == TeamBlockType.Individual)
			{
				wasModified1.Should().Be.True();
				wasModified2.Should().Be.True();
			}
			else
			{
				wasModified1.Should().Be.False();
				wasModified2.Should().Be.False();
			}
		}

		[TestCase(TeamBlockType.Team, false)]
		[TestCase(TeamBlockType.Block, false)]
		[TestCase(TeamBlockType.TeamAndBlock, false)]
		[TestCase(TeamBlockType.Individual, false)]
		[TestCase(TeamBlockType.Team, true)]
		[TestCase(TeamBlockType.Block, true)]
		[TestCase(TeamBlockType.TeamAndBlock, true)]
		[TestCase(TeamBlockType.Individual, true)]
		public void ShouldNotOverwriteAbsencePartOfDayWhenGettingBackToLegalState(TeamBlockType teamBlockType, bool haveAbsence)
		{
			var firstDay = new DateOnly(2015, 10, 12);
			var period = DateOnlyPeriod.CreateWithNumberOfWeeks(firstDay, 1);
			var absence = new Absence();
			var activity = new Activity("_");
			var contract = new ContractWithMaximumTolerance();
			var skill = new Skill().WithId().For(activity).IsOpen();
			var scenario = new Scenario("_");
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet, contract, skill).WithSchedulePeriodOneWeek(firstDay);
			agent.SchedulePeriod(firstDay).SetDaysOff(2);
			var skillDays = skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay, 5, 5, 1, 1, 5, 5, 5);	
			var persistableScheduleData = new List<IPersistableScheduleData>();
			for (var i = 0; i < 7; i++)
			{
				var day = firstDay.AddDays(i);
				var personAssignment = new PersonAssignment(agent, scenario, day).ShiftCategory(shiftCategory).WithLayer(activity, new TimePeriod(8, 16));
				if (i == 2 || i == 3)
				{
					personAssignment.SetDayOff(new DayOffTemplate());
				}
				persistableScheduleData.Add(personAssignment);

				if (i == 5 && haveAbsence)
				{
					var personAbsence = new PersonAbsence(agent, scenario, new AbsenceLayer(absence, day.ToDateTimePeriod(new TimePeriod(8, 9), TimeZoneInfo.Utc)));
					persistableScheduleData.Add(personAbsence);
				}
			}

			var stateHolder = SchedulerStateHolder.Fill(scenario, period, new[] { agent }, persistableScheduleData, skillDays);
			var optPrefs = new OptimizationPreferences {General = {ScheduleTag = new ScheduleTag()}, Extra = teamBlockType.CreateExtraPreferences()};
			var dayOffsPreferences = new DaysOffPreferences {UseFullWeekendsOff = true, FullWeekendsOffValue = new MinMax<int>(1, 1)};

			Target.Execute(period, new[] { agent }, optPrefs, new FixedDayOffOptimizationPreferenceProvider(dayOffsPreferences), new NoOptimizationCallback());

			if (teamBlockType == TeamBlockType.Individual && !haveAbsence)
			{
				stateHolder.Schedules[agent].ScheduledDay(firstDay.AddDays(5)).HasDayOff().Should().Be.True();
				stateHolder.Schedules[agent].ScheduledDay(firstDay.AddDays(6)).HasDayOff().Should().Be.True();
			}
			else
			{
				stateHolder.Schedules[agent].ScheduledDay(firstDay.AddDays(5)).HasDayOff().Should().Be.False();
				stateHolder.Schedules[agent].ScheduledDay(firstDay.AddDays(6)).HasDayOff().Should().Be.False();
			}
		}

		[Test]
		public void ShouldReScheduleWhiteSpotsAfterGetBackToLegalStateClassic()
		{
			var firstDay = new DateOnly(2015, 10, 12); //mon
			var period = DateOnlyPeriod.CreateWithNumberOfWeeks(firstDay, 1);
			var activity = new Activity("_");
			var contract = new ContractWithMaximumTolerance();
			var skill = new Skill().WithId().For(activity).IsOpen();
			var scenario = new Scenario("_");
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet, contract, skill).WithSchedulePeriodOneWeek(firstDay);
			agent.SchedulePeriod(firstDay).SetDaysOff(1);
			var skillDays = skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay,
				5,
				5,
				1,
				1,
				5,
				5,
				5);
			var asses = Enumerable.Range(0, 7).Select(i => new PersonAssignment(agent, scenario, firstDay.AddDays(i)).ShiftCategory(shiftCategory).WithLayer(activity, new TimePeriod(8, 16))).ToArray();
			asses[2].SetDayOff(new DayOffTemplate());
			asses[3].SetDayOff(new DayOffTemplate());
			var stateHolder = SchedulerStateHolder.Fill(scenario, period, new[] { agent }, asses, skillDays);
			var optPrefs = new OptimizationPreferences
			{
				General = { ScheduleTag = new ScheduleTag() },
				Extra = new ExtraPreferences { UseTeams = false, UseTeamBlockOption = false }
			};
			var dayOffsPreferences = new DaysOffPreferences
			{
				UseFullWeekendsOff = true,
				FullWeekendsOffValue = new MinMax<int>(1, 1)
			};

			Target.Execute(period, new[] { agent }, optPrefs, new FixedDayOffOptimizationPreferenceProvider(dayOffsPreferences), new NoOptimizationCallback());

			stateHolder.Schedules[agent].ScheduledDay(firstDay.AddDays(2)).IsScheduled().Should().Be.True();
			stateHolder.Schedules[agent].ScheduledDay(firstDay.AddDays(3)).IsScheduled().Should().Be.True();
		}

		[Test]
		public void ShouldGetBackToLegalStateWorkShifts()
		{
			var firstDay = new DateOnly(2015, 10, 12); //mon
			var period = DateOnlyPeriod.CreateWithNumberOfWeeks(firstDay, 1);
			var activity = new Activity("_");
			var contract = new Contract("_")
			{
				WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(10), TimeSpan.FromHours(44), TimeSpan.FromHours(1), TimeSpan.FromHours(16)),
				NegativePeriodWorkTimeTolerance = TimeSpan.FromHours(4)
			};
			var skill = new Skill().WithId().For(activity).IsOpen();
			var scenario = new Scenario("_");
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet, contract, skill).WithSchedulePeriodOneWeek(firstDay);
			agent.SchedulePeriod(firstDay).SetDaysOff(1);
			var skillDays = skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay,
				5,
				5,
				5,
				5,
				5,
				1,
				1);
			var asses = Enumerable.Range(0, 7).Select(i => new PersonAssignment(agent, scenario, firstDay.AddDays(i)).ShiftCategory(shiftCategory).WithLayer(activity, new TimePeriod(8, 17))).ToArray();
			asses[5].SetDayOff(new DayOffTemplate());
			asses[6].SetDayOff(new DayOffTemplate());
			var stateHolder = SchedulerStateHolder.Fill(scenario, period, new[] { agent }, asses, skillDays);
			var optPrefs = new OptimizationPreferences
			{
				General = { ScheduleTag = new ScheduleTag() },
				Extra = new ExtraPreferences { UseTeams = false, UseTeamBlockOption = false }
			};

			Target.Execute(period, new[] { agent }, optPrefs, new FixedDayOffOptimizationPreferenceProvider(new DaysOffPreferences()), new NoOptimizationCallback());

			stateHolder.Schedules[agent].ScheduledDay(firstDay)
					.PersonAssignment()
					.ShiftLayers.First()
					.Period.ElapsedTime()
					.Should()
					.Be.EqualTo(TimeSpan.FromHours(8));
		}

		[Test]
		public void ShouldNotMoveDayOffWhenBreakingKeepRotation()
		{
			var firstDay = new DateOnly(2015, 10, 12); //mon
			var period = DateOnlyPeriod.CreateWithNumberOfWeeks(firstDay, 1);
			var activity = new Activity("_");
			var skill = new Skill().WithId().For(activity).IsOpen();
			var scenario = new Scenario("_");
			var shiftCategory = new ShiftCategory("_").WithId();
			var dayOffTemplate = new DayOffTemplate();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var team = new Team { Site = new Site("_") };
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet, team, skill).WithSchedulePeriodOneWeek(firstDay);
			agent.SchedulePeriod(firstDay).SetDaysOff(1);
			var skillDays = skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay, 5, 1, 5, 5, 5, 25, 5);
			var scheduleDatas = new List<IScheduleData>();
			for (var i = 0; i < 7; i++)
			{
				var ass = new PersonAssignment(agent, scenario, firstDay.AddDays(i)).ShiftCategory(shiftCategory).WithLayer(activity, new TimePeriod(8, 16));
				var rotationRestriction = new RotationRestriction();
				if (i == 5)
				{
					ass.SetDayOff(dayOffTemplate); //saturday
					rotationRestriction.DayOffTemplate = dayOffTemplate;
				}
				else
				{
					rotationRestriction.ShiftCategory = shiftCategory;
				}
				scheduleDatas.Add(ass);
				scheduleDatas.Add(new ScheduleDataRestriction(agent, rotationRestriction, firstDay.AddDays(i)));
			}
			var stateHolder = SchedulerStateHolder.Fill(scenario, period, new[] { agent }, scheduleDatas, skillDays);
			var optPrefs = new OptimizationPreferences { General = {ScheduleTag = new ScheduleTag(), UseRotations = true, RotationsValue = 0.9}};

			Target.Execute(period, new[] { agent }, optPrefs, new FixedDayOffOptimizationPreferenceProvider(new DaysOffPreferences()), new NoOptimizationCallback());

			stateHolder.Schedules[agent].ScheduledDay(firstDay.AddDays(5)).HasDayOff()
				.Should().Be.True();//saturday
		}

		[Test]
		public void ShouldNotMoveDayOffWhenUsingConsecutiveWorkDaysAndHavingDayOffPreference100Percent()
		{
			var firstDay = new DateOnly(2015, 10, 12); 
			var period = DateOnlyPeriod.CreateWithNumberOfWeeks(firstDay, 1);
			var activity = new Activity("_");
			var skill = new Skill().WithId().For(activity).IsOpen();
			var scenario = new Scenario("_");
			var shiftCategory = new ShiftCategory("_").WithId();
			var dayOffTemplate = new DayOffTemplate();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet, skill).WithSchedulePeriodOneWeek(firstDay);
			agent.SchedulePeriod(firstDay).SetDaysOff(2);
			var skillDays = skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay, 5, 5, 5, 1, 25, 5, 5);
			var scheduleDatas = new List<IPersistableScheduleData>();
			for (var i = 0; i < 7; i++)
			{
				var ass = new PersonAssignment(agent, scenario, firstDay.AddDays(i)).ShiftCategory(shiftCategory).WithLayer(activity, new TimePeriod(8, 16));
				if (i == 4 || i == 6)
				{
					var preferenceRestriction = new PreferenceRestriction();
					ass.SetDayOff(dayOffTemplate); 
					preferenceRestriction.DayOffTemplate = dayOffTemplate;
					scheduleDatas.Add(new PreferenceDay(agent, firstDay.AddDays(i), preferenceRestriction));
				}
	
				scheduleDatas.Add(ass);
			}
			var stateHolder = SchedulerStateHolder.Fill(scenario, period, new[] { agent }, scheduleDatas, skillDays);
			var optPrefs = new OptimizationPreferences { General = {ScheduleTag = new ScheduleTag(), UsePreferences= true, PreferencesValue = 1.0d}};
			var dayOffPreferences = new DaysOffPreferences {UseConsecutiveWorkdays = true, ConsecutiveWorkdaysValue = new MinMax<int>(1, 3)};

			Target.Execute(period, new[] { agent }, optPrefs, new FixedDayOffOptimizationPreferenceProvider(dayOffPreferences), new NoOptimizationCallback());

			stateHolder.Schedules[agent].ScheduledDay(firstDay.AddDays(4)).HasDayOff().Should().Be.True();
			stateHolder.Schedules[agent].ScheduledDay(firstDay.AddDays(6)).HasDayOff().Should().Be.True();
		}

		[TestCase(true, ExpectedResult = true)]
		[TestCase(false, ExpectedResult = false)]
		public bool ShouldNotMoveLockedDayOff(bool locked)
		{
			var firstDay = new DateOnly(2015, 10, 12); //mon
			var period = DateOnlyPeriod.CreateWithNumberOfWeeks(firstDay, 1);
			var activity = new Activity("_");
			var skill = new Skill().WithId().For(activity).IsOpen();
			var scenario = new Scenario("_");
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var team = new Team { Site = new Site("_") };
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet, team, skill).WithSchedulePeriodOneWeek(firstDay);
			agent.SchedulePeriod(firstDay).SetDaysOff(1);
			var skillDays = skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay, 5, 1, 5, 5, 5, 25, 5);
			var asses = Enumerable.Range(0, 7).Select(i => new PersonAssignment(agent, scenario, firstDay.AddDays(i)).ShiftCategory(shiftCategory).WithLayer(activity, new TimePeriod(8, 16))).ToArray();
			asses[5].SetDayOff(new DayOffTemplate()); //saturday
			var stateHolder = SchedulerStateHolder.Fill(scenario, period, new[] { agent }, asses, skillDays);
			var optPrefs = new OptimizationPreferences { General = { ScheduleTag = new ScheduleTag() } };
			if(locked)
			{
				LockManager().AddLock(agent, firstDay.AddDays(5), LockType.Normal);
			}

			Target.Execute(period, new[] { agent }, optPrefs, new FixedDayOffOptimizationPreferenceProvider(new DaysOffPreferences()), new NoOptimizationCallback());

			return stateHolder.Schedules[agent].ScheduledDay(firstDay.AddDays(5)).HasDayOff();//saturday
		}

		[Test]
		public void DaysOffBackToLegalStateShouldNotMoveDayOffFromClosedDays()
		{
			var firstDay = new DateOnly(2015, 10, 12); //mon
			var period = DateOnlyPeriod.CreateWithNumberOfWeeks(firstDay, 1);
			var activity = new Activity("_");
			var timePeriod = new TimePeriod(8, 16);
			var skill = new Skill().WithId().For(activity).IsOpen(timePeriod, timePeriod, timePeriod, timePeriod, timePeriod, timePeriod); //closed on sundays
			var scenario = new Scenario("_");
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var team = new Team { Site = new Site("_") };
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet, team, skill).WithSchedulePeriodOneWeek(firstDay);
			agent.SchedulePeriod(firstDay).SetDaysOff(2);
			var skillDays = skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay, 5, 1, 5, 5, 5, 25);
			var asses = Enumerable.Range(0, 7).Select(i => new PersonAssignment(agent, scenario, firstDay.AddDays(i)).ShiftCategory(shiftCategory).WithLayer(activity, new TimePeriod(8, 16))).ToArray();
			var doTemplate = new DayOffTemplate();
			asses[5].SetDayOff(doTemplate); //saturday
			asses[6].SetDayOff(doTemplate); //sunday
			var stateHolder = SchedulerStateHolder.Fill(scenario, period, new[] { agent }, asses, skillDays);
			var optPrefs = new OptimizationPreferences { General = { ScheduleTag = new ScheduleTag() }};
			//locking saturday so sunday is the only day to remove to comply with DO rules
			LockManager().AddLock(agent, firstDay.AddDays(5), LockType.Normal);
			var doPrefs = new DaysOffPreferences() {UseWeekEndDaysOff = true, WeekEndDaysOffValue = new MinMax<int>(1, 1)};
			
			Target.Execute(period, new[] { agent }, optPrefs, new FixedDayOffOptimizationPreferenceProvider(doPrefs), new NoOptimizationCallback());

			stateHolder.Schedules[agent].ScheduledDay(firstDay.AddDays(6)).HasDayOff().Should().Be.True();//closed sunday should not move
		}

		public DayOffOptimizationDesktopTest(ResourcePlannerTestParameters resourcePlannerTestParameters) : base(resourcePlannerTestParameters)
		{
			_resourcePlannerTestParameters = resourcePlannerTestParameters;
		}
	}
}