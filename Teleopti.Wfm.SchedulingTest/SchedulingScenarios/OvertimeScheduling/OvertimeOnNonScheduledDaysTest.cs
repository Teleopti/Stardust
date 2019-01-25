using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.Overtime;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Scheduling;


namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.OvertimeScheduling
{
	[DomainTest]
	public class OvertimeOnNonScheduledDaysTest : OvertimeSchedulingScenario
	{
		public ScheduleOvertime Target;
		public Func<ISchedulerStateHolder> SchedulerStateHolderFrom;

		[Test]
		public void ShouldPlaceOvertimeOnEmptyDayWithDemand()
		{
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var dateOnly = DateOnly.Today;
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(7, 0, 8, 0, 60), new TimePeriodWithSegment(15, 0, 16, 0, 60), new ShiftCategory("_").WithId()));
			var contract = new Contract("_") { WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(10), TimeSpan.FromHours(83), TimeSpan.FromHours(1), TimeSpan.FromHours(16)) };
			var definitionSet = new MultiplicatorDefinitionSet("overtime", MultiplicatorType.Overtime);
			contract.AddMultiplicatorDefinitionSetCollection(definitionSet);
			var skill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
			var skillDay = skill.CreateSkillDayWithDemand(scenario, dateOnly, TimeSpan.FromMinutes(60));
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(contract, skill).WithSchedulePeriodOneDay(dateOnly);
			var overtimePreference = new OvertimePreferences {OvertimeType = definitionSet, ShiftBagToUse = new RuleSetBag(ruleSet), ScheduleTag = new ScheduleTag()};
			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, new DateOnlyPeriod(dateOnly, dateOnly), new[] {agent}, Enumerable.Empty<IScheduleData>(), skillDay);

			Target.Execute(overtimePreference, new NoSchedulingProgress(), new [] {stateHolder.Schedules[agent].ScheduledDay(dateOnly)});

			stateHolder.Schedules[agent].ScheduledDay(dateOnly).PersonAssignment(true).OvertimeActivities()
				.Should().Not.Be.Empty();
		}

		[Test]
		public void ShouldNotPlaceOvertimeOnEmptyDayWithDemandIfPrefHasNoShiftBag()
		{
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var dateOnly = DateOnly.Today;
			var contract = new Contract("_") { WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(10), TimeSpan.FromHours(83), TimeSpan.FromHours(1), TimeSpan.FromHours(16)) };
			contract.AddMultiplicatorDefinitionSetCollection(new MultiplicatorDefinitionSet("overtime", MultiplicatorType.Overtime));
			var skill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
			var skillDay = skill.CreateSkillDayWithDemand(scenario, dateOnly, TimeSpan.FromMinutes(60));
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(contract, skill).WithSchedulePeriodOneDay(dateOnly);
			var overtimePreference = new OvertimePreferences { ScheduleTag = new ScheduleTag() };
			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, new DateOnlyPeriod(dateOnly, dateOnly), new[] { agent }, Enumerable.Empty<IScheduleData>(), skillDay);

			Target.Execute(overtimePreference, new NoSchedulingProgress(), new[] { stateHolder.Schedules[agent].ScheduledDay(dateOnly) });

			stateHolder.Schedules[agent].ScheduledDay(dateOnly).PersonAssignment(true).OvertimeActivities()
				.Should().Be.Empty();
		}

		[Test]
		public void ShouldNotPlaceOvertimeOnEmptyDayIfAgentHasNoMultiplicatorSetWithOvertime()
		{
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var dateOnly = DateOnly.Today;
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(7, 0, 8, 0, 60), new TimePeriodWithSegment(15, 0, 16, 0, 60), new ShiftCategory("_").WithId()));
			var contract = new Contract("_") { WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(10), TimeSpan.FromHours(83), TimeSpan.FromHours(1), TimeSpan.FromHours(16)) };
			contract.AddMultiplicatorDefinitionSetCollection(new MultiplicatorDefinitionSet("non overtime", MultiplicatorType.OBTime));
			var skill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
			var skillDay = skill.CreateSkillDayWithDemand(scenario, dateOnly, TimeSpan.FromMinutes(60));
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(contract, skill).WithSchedulePeriodOneDay(dateOnly);
			var overtimePreference = new OvertimePreferences { ShiftBagToUse = new RuleSetBag(ruleSet), ScheduleTag = new ScheduleTag() };
			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, new DateOnlyPeriod(dateOnly, dateOnly), new[] { agent }, Enumerable.Empty<IScheduleData>(), skillDay);

			Target.Execute(overtimePreference, new NoSchedulingProgress(), new[] { stateHolder.Schedules[agent].ScheduledDay(dateOnly) });

			stateHolder.Schedules[agent].ScheduledDay(dateOnly).PersonAssignment(true).OvertimeActivities()
				.Should().Be.Empty();
		}

		[Test]
		public void ShouldPlaceOvertimeLayersWithCorrectDefinitionSet()
		{
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var dateOnly = DateOnly.Today;
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(7, 0, 8, 0, 60), new TimePeriodWithSegment(15, 0, 16, 0, 60), new ShiftCategory("_").WithId()));
			var contract = new Contract("_") { WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(10), TimeSpan.FromHours(83), TimeSpan.FromHours(1), TimeSpan.FromHours(16)) };
			var definitionSet = new MultiplicatorDefinitionSet("overtime", MultiplicatorType.Overtime);
			contract.AddMultiplicatorDefinitionSetCollection(definitionSet);
			var skill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
			var skillDay = skill.CreateSkillDayWithDemand(scenario, dateOnly, TimeSpan.FromMinutes(60));
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(contract, skill).WithSchedulePeriodOneDay(dateOnly);
			var overtimePreference = new OvertimePreferences {OvertimeType = definitionSet, ShiftBagToUse = new RuleSetBag(ruleSet), ScheduleTag = new ScheduleTag() };
			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, new DateOnlyPeriod(dateOnly, dateOnly), new[] { agent }, Enumerable.Empty<IScheduleData>(), skillDay);

			Target.Execute(overtimePreference, new NoSchedulingProgress(), new[] { stateHolder.Schedules[agent].ScheduledDay(dateOnly) });

			stateHolder.Schedules[agent].ScheduledDay(dateOnly).PersonAssignment(true).OvertimeActivities().Single().DefinitionSet
				.Should().Be.SameInstanceAs(definitionSet);
		}

		[Test]
		public void ShouldNotPlaceOvertimeWhenAgentDontHaveCorrectDefinitionSet()
		{
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var dateOnly = DateOnly.Today;
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(7, 0, 8, 0, 60), new TimePeriodWithSegment(15, 0, 16, 0, 60), new ShiftCategory("_").WithId()));
			var contract = new Contract("_") { WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(10), TimeSpan.FromHours(83), TimeSpan.FromHours(1), TimeSpan.FromHours(16)) };
			var definitionSet1 = new MultiplicatorDefinitionSet("overtime1", MultiplicatorType.Overtime);
			var definitionSet2 = new MultiplicatorDefinitionSet("overtime2", MultiplicatorType.Overtime);
			contract.AddMultiplicatorDefinitionSetCollection(definitionSet2);
			var skill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
			var skillDay = skill.CreateSkillDayWithDemand(scenario, dateOnly, TimeSpan.FromMinutes(60));
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(contract, skill).WithSchedulePeriodOneDay(dateOnly);
			var overtimePreference = new OvertimePreferences { OvertimeType = definitionSet1, ShiftBagToUse = new RuleSetBag(ruleSet), ScheduleTag = new ScheduleTag() };
			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, new DateOnlyPeriod(dateOnly, dateOnly), new[] { agent }, Enumerable.Empty<IScheduleData>(), skillDay);

			Target.Execute(overtimePreference, new NoSchedulingProgress(), new[] { stateHolder.Schedules[agent].ScheduledDay(dateOnly) });

			stateHolder.Schedules[agent].ScheduledDay(dateOnly).PersonAssignment(true).OvertimeActivities()
				.Should().Be.Empty();
		}

		[Test]
		public void ShouldNotPlaceOvertimeOnDayWithAbsence()
		{
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var dateOnly = DateOnly.Today;
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(7, 0, 8, 0, 60), new TimePeriodWithSegment(15, 0, 16, 0, 60), new ShiftCategory("_").WithId()));
			var contract = new Contract("_") { WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(10), TimeSpan.FromHours(83), TimeSpan.FromHours(1), TimeSpan.FromHours(16)) };
			contract.AddMultiplicatorDefinitionSetCollection(new MultiplicatorDefinitionSet("overtime", MultiplicatorType.Overtime));
			var skill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
			var skillDay = skill.CreateSkillDayWithDemand(scenario, dateOnly, TimeSpan.FromMinutes(60));
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(contract, skill).WithSchedulePeriodOneDay(dateOnly);
			var overtimePreference = new OvertimePreferences { ShiftBagToUse = new RuleSetBag(ruleSet), ScheduleTag = new ScheduleTag() };
			var absencePeriod = new DateTimePeriod(dateOnly.Year, dateOnly.Month, dateOnly.Day, 10, dateOnly.Year, dateOnly.Month, dateOnly.Day, 12);
			var personAbsence = PersonAbsenceFactory.CreatePersonAbsence(agent, scenario, absencePeriod);
			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, new DateOnlyPeriod(dateOnly, dateOnly), new[] { agent }, new List<IScheduleData> {personAbsence}, skillDay);

			Target.Execute(overtimePreference, new NoSchedulingProgress(), new[] { stateHolder.Schedules[agent].ScheduledDay(dateOnly) });

			stateHolder.Schedules[agent].ScheduledDay(dateOnly).PersonAssignment(true).OvertimeActivities()
				.Should().Be.Empty();
		}

		[Test]
		public void ShouldPlaceOvertimeOnDayWithDayOff()
		{
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var dateOnly = DateOnly.Today;
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(7, 0, 8, 0, 60), new TimePeriodWithSegment(15, 0, 16, 0, 60), new ShiftCategory("_").WithId()));
			var contract = new ContractWithMaximumTolerance();
			var definitionSet = new MultiplicatorDefinitionSet("overtime", MultiplicatorType.Overtime);
			contract.AddMultiplicatorDefinitionSetCollection(definitionSet);
			var skill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
			var skillDay = skill.CreateSkillDayWithDemand(scenario, dateOnly, TimeSpan.FromMinutes(60));
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(contract, skill).WithSchedulePeriodOneDay(dateOnly);
			var overtimePreference = new OvertimePreferences {OvertimeType = definitionSet, ShiftBagToUse = new RuleSetBag(ruleSet), ScheduleTag = new ScheduleTag() };
			var ass = new PersonAssignment(agent, scenario, dateOnly).WithDayOff();
			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, new DateOnlyPeriod(dateOnly, dateOnly), new[] { agent }, new[] {ass}, skillDay);

			Target.Execute(overtimePreference, new NoSchedulingProgress(), new[] { stateHolder.Schedules[agent].ScheduledDay(dateOnly) });

			stateHolder.Schedules[agent].ScheduledDay(dateOnly).PersonAssignment(true).OvertimeActivities()
				.Should().Not.Be.Empty();
			stateHolder.Schedules[agent].ScheduledDay(dateOnly).HasDayOff()
				.Should().Be(true);
		}

		[Test]
		public void ShouldNotPlaceOvertimeOnDayWithAssignmentWithLayers()
		{
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var dateOnly = DateOnly.Today;
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(7, 0, 8, 0, 60), new TimePeriodWithSegment(15, 0, 16, 0, 60), new ShiftCategory("_").WithId()));
			var contract = new Contract("_") { WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(10), TimeSpan.FromHours(83), TimeSpan.FromHours(1), TimeSpan.FromHours(16)) };
			var definitionSet = new MultiplicatorDefinitionSet("overtime", MultiplicatorType.Overtime);
			contract.AddMultiplicatorDefinitionSetCollection(definitionSet);
			var skill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
			var skillDay = skill.CreateSkillDayWithDemand(scenario, dateOnly, TimeSpan.FromMinutes(60));
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(contract, skill).WithSchedulePeriodOneDay(dateOnly);
			var overtimePreference = new OvertimePreferences
			{
				OvertimeType = definitionSet,
				ShiftBagToUse = new RuleSetBag(ruleSet),
				ScheduleTag = new ScheduleTag(),
				SelectedTimePeriod = new TimePeriod(0,0,0,0) //to prevent overtime isn't added at beginning and ending of shift
			};
			var ass = new PersonAssignment(agent, scenario, dateOnly);
			ass.AddActivity(new Activity("_"), new DateOnlyPeriod(dateOnly, dateOnly).ToDateTimePeriod(agent.PermissionInformation.DefaultTimeZone()));
			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, new DateOnlyPeriod(dateOnly, dateOnly), new[] { agent }, new[] {ass}, skillDay);

			Target.Execute(overtimePreference, new NoSchedulingProgress(), new[] { stateHolder.Schedules[agent].ScheduledDay(dateOnly) });

			stateHolder.Schedules[agent].ScheduledDay(dateOnly).PersonAssignment(true).OvertimeActivities()
				.Should().Be.Empty();
		}

		[Test]
		public void ShouldPlaceOvertimeOnDayWithAssignmentWithNoLayers()
		{
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var dateOnly = DateOnly.Today;
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(7, 0, 8, 0, 60), new TimePeriodWithSegment(15, 0, 16, 0, 60), new ShiftCategory("_").WithId()));
			var contract = new Contract("_") { WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(10), TimeSpan.FromHours(83), TimeSpan.FromHours(1), TimeSpan.FromHours(16)) };
			var definitionSet = new MultiplicatorDefinitionSet("overtime", MultiplicatorType.Overtime);
			contract.AddMultiplicatorDefinitionSetCollection(definitionSet);
			var skill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
			var skillDay = skill.CreateSkillDayWithDemand(scenario, dateOnly, TimeSpan.FromMinutes(60));
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(contract, skill).WithSchedulePeriodOneDay(dateOnly);
			var overtimePreference = new OvertimePreferences
			{
				OvertimeType = definitionSet,
				ShiftBagToUse = new RuleSetBag(ruleSet),
				ScheduleTag = new ScheduleTag()
			};
			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, new DateOnlyPeriod(dateOnly, dateOnly), new[] { agent }, new[] { new PersonAssignment(agent, scenario, dateOnly)}, skillDay);

			Target.Execute(overtimePreference, new NoSchedulingProgress(), new[] { stateHolder.Schedules[agent].ScheduledDay(dateOnly) });

			stateHolder.Schedules[agent].ScheduledDay(dateOnly).PersonAssignment(true).OvertimeActivities()
				.Should().Not.Be.Empty();
		}

		[Test]
		public void ShouldConsiderWeeklyMaxWorkTime()
		{
			var scenario = new Scenario("_");
			var activity = new Activity("_") {InWorkTime = true};
			var dateOnly = DateOnly.Today;
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(7, 0, 8, 0, 60), new TimePeriodWithSegment(15, 0, 16, 0, 60), new ShiftCategory("_").WithId()));
			var contract = new Contract("_") { WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(1), TimeSpan.FromHours(1), TimeSpan.FromHours(1), TimeSpan.FromHours(16)) };
			var definitionSet = new MultiplicatorDefinitionSet("overtime", MultiplicatorType.Overtime);
			contract.AddMultiplicatorDefinitionSetCollection(definitionSet);
			var skill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
			var skillDay = skill.CreateSkillDayWithDemand(scenario, dateOnly, TimeSpan.FromMinutes(60));
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(contract, skill).WithSchedulePeriodOneDay(dateOnly);
			var overtimePreference = new OvertimePreferences {AllowBreakMaxWorkPerWeek = false, OvertimeType = definitionSet, ShiftBagToUse = new RuleSetBag(ruleSet), ScheduleTag = new ScheduleTag() };
			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, new DateOnlyPeriod(dateOnly, dateOnly), new[] { agent }, Enumerable.Empty<IScheduleData>(), skillDay);

			Target.Execute(overtimePreference, new NoSchedulingProgress(), new[] { stateHolder.Schedules[agent].ScheduledDay(dateOnly) });

			stateHolder.Schedules[agent].ScheduledDay(dateOnly).PersonAssignment(true).OvertimeActivities()
				.Should().Be.Empty();
		}

		[Test]
		public void ShouldGetPossibleShiftToPleaseWeeklyMaxMorkTime()
		{
			var scenario = new Scenario("_");
			var activity = new Activity("_") { InWorkTime = true };
			var dateOnly = DateOnly.Today;
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(7, 0, 8, 0, 60), new TimePeriodWithSegment(15, 0, 15, 0, 60), new ShiftCategory("_").WithId()));
			var contract = new Contract("_") { WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(1), TimeSpan.FromHours(7), TimeSpan.FromHours(1), TimeSpan.FromHours(16)) };
			var definitionSet = new MultiplicatorDefinitionSet("overtime", MultiplicatorType.Overtime);
			contract.AddMultiplicatorDefinitionSetCollection(definitionSet);
			var skill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
			var skillDay = skill.CreateSkillDayWithDemand(scenario, dateOnly, TimeSpan.FromMinutes(60));
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(contract, skill).WithSchedulePeriodOneDay(dateOnly);
			var overtimePreference = new OvertimePreferences { AllowBreakMaxWorkPerWeek = false, OvertimeType = definitionSet, ShiftBagToUse = new RuleSetBag(ruleSet), ScheduleTag = new ScheduleTag() };
			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, new DateOnlyPeriod(dateOnly, dateOnly), new[] { agent }, Enumerable.Empty<IScheduleData>(), skillDay);

			Target.Execute(overtimePreference, new NoSchedulingProgress(), new[] { stateHolder.Schedules[agent].ScheduledDay(dateOnly) });

			stateHolder.Schedules[agent].ScheduledDay(dateOnly).PersonAssignment(true).OvertimeActivities()
				.Should().Not.Be.Empty();
		}

		[Test]
		public void ShouldNotCreateAnyKindOfLayersIfRollbacked()
		{
			var scenario = new Scenario("_");
			var activity = new Activity("_") { InWorkTime = true };
			var dateOnly = DateOnly.Today;
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(7, 0, 8, 0, 60), new TimePeriodWithSegment(15, 0, 16, 0, 60), new ShiftCategory("_").WithId()));
			var contract = new Contract("_") { WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(1), TimeSpan.FromHours(1), TimeSpan.FromHours(1), TimeSpan.FromHours(16)) };
			var definitionSet = new MultiplicatorDefinitionSet("overtime", MultiplicatorType.Overtime);
			contract.AddMultiplicatorDefinitionSetCollection(definitionSet);
			var skill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
			var skillDay = skill.CreateSkillDayWithDemand(scenario, dateOnly, TimeSpan.FromMinutes(60));
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(contract, skill).WithSchedulePeriodOneDay(dateOnly);
			var overtimePreference = new OvertimePreferences { AllowBreakMaxWorkPerWeek = false, OvertimeType = definitionSet, ShiftBagToUse = new RuleSetBag(ruleSet), ScheduleTag = new ScheduleTag() };
			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, new DateOnlyPeriod(dateOnly, dateOnly), new[] { agent }, Enumerable.Empty<IScheduleData>(), skillDay);

			Target.Execute(overtimePreference, new NoSchedulingProgress(), new[] { stateHolder.Schedules[agent].ScheduledDay(dateOnly) });

			stateHolder.Schedules[agent].ScheduledDay(dateOnly).PersonAssignment(true).ShiftLayers
				.Should().Be.Empty();
		}

		[Test]
		public void ShouldNotConsiderWeeklyMaxWorkTime()
		{
			var scenario = new Scenario("_");
			var activity = new Activity("_") { InWorkTime = true };
			var dateOnly = new DateOnly(2015, 4, 29);
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(7, 0, 8, 0, 60), new TimePeriodWithSegment(15, 0, 16, 0, 60), new ShiftCategory("_").WithId()));
			var contract = new Contract("_") { WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(1), TimeSpan.FromHours(1), TimeSpan.FromHours(1), TimeSpan.FromHours(16)) };
			var definitionSet = new MultiplicatorDefinitionSet("overtime", MultiplicatorType.Overtime);
			contract.AddMultiplicatorDefinitionSetCollection(definitionSet);
			var skill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
			var skillDay = skill.CreateSkillDayWithDemand(scenario, dateOnly, TimeSpan.FromMinutes(60));
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(contract, skill).WithSchedulePeriodOneDay(dateOnly);
			var overtimePreference = new OvertimePreferences { AllowBreakMaxWorkPerWeek = true, OvertimeType = definitionSet, ShiftBagToUse = new RuleSetBag(ruleSet), ScheduleTag = new ScheduleTag() };
			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, new DateOnlyPeriod(dateOnly, dateOnly), new[] { agent }, Enumerable.Empty<IScheduleData>(), skillDay);

			Target.Execute(overtimePreference, new NoSchedulingProgress(), new[] { stateHolder.Schedules[agent].ScheduledDay(dateOnly) });

			stateHolder.Schedules[agent].ScheduledDay(dateOnly).PersonAssignment(true).OvertimeActivities()
				.Should().Not.Be.Empty();
		}

		[Test]
		public void ShouldNotConsiderWeeklyMaxWorkTime_OnSpecificDay_UsedToFailButNowGreenLetsKeepItAndSeeIfItIsForeverGreen()
		{
			var scenario = new Scenario("_");
			var activity = new Activity("_") { InWorkTime = true };
			var dateOnly = new DateOnly(2016, 5, 2);
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(7, 0, 8, 0, 60), new TimePeriodWithSegment(15, 0, 16, 0, 60), new ShiftCategory("_").WithId()));
			var contract = new Contract("_") { WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(1), TimeSpan.FromHours(1), TimeSpan.FromHours(1), TimeSpan.FromHours(16)) };
			var definitionSet = new MultiplicatorDefinitionSet("overtime", MultiplicatorType.Overtime);
			contract.AddMultiplicatorDefinitionSetCollection(definitionSet);
			var skill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
			var skillDay = skill.CreateSkillDayWithDemand(scenario, dateOnly, TimeSpan.FromMinutes(60));
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(contract, skill).WithSchedulePeriodOneDay(dateOnly);
			var overtimePreference = new OvertimePreferences { AllowBreakMaxWorkPerWeek = true, OvertimeType = definitionSet, ShiftBagToUse = new RuleSetBag(ruleSet), ScheduleTag = new ScheduleTag() };
			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, new DateOnlyPeriod(dateOnly, dateOnly), new[] { agent }, Enumerable.Empty<IScheduleData>(), skillDay);

			Target.Execute(overtimePreference, new NoSchedulingProgress(), new[] { stateHolder.Schedules[agent].ScheduledDay(dateOnly) });

			stateHolder.Schedules[agent].ScheduledDay(dateOnly).PersonAssignment(true).OvertimeActivities()
				.Should().Not.Be.Empty();
		}

		[Test]
		public void ShouldSetCorrectTag()
		{
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var dateOnly = DateOnly.Today;
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(7, 0, 8, 0, 60), new TimePeriodWithSegment(15, 0, 16, 0, 60), new ShiftCategory("_").WithId()));
			var contract = new Contract("_") { WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(10), TimeSpan.FromHours(83), TimeSpan.FromHours(1), TimeSpan.FromHours(16)) };
			var definitionSet = new MultiplicatorDefinitionSet("overtime", MultiplicatorType.Overtime);
			contract.AddMultiplicatorDefinitionSetCollection(definitionSet);
			var skill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
			var skillDay = skill.CreateSkillDayWithDemand(scenario, dateOnly, TimeSpan.FromMinutes(60));
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(contract, skill).WithSchedulePeriodOneDay(dateOnly);
			var scheduleTag = new ScheduleTag();
			var overtimePreference = new OvertimePreferences { OvertimeType = definitionSet, ShiftBagToUse = new RuleSetBag(ruleSet), ScheduleTag = scheduleTag };
			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, new DateOnlyPeriod(dateOnly, dateOnly), new[] { agent }, Enumerable.Empty<IScheduleData>(), skillDay);

			Target.Execute(overtimePreference, new NoSchedulingProgress(), new[] { stateHolder.Schedules[agent].ScheduledDay(dateOnly) });

			stateHolder.Schedules[agent].ScheduledDay(dateOnly).ScheduleTag()
				.Should().Be.SameInstanceAs(scheduleTag);
		}

		[Test]
		public void ShouldConsiderNightlyRest()
		{
			var scenario = new Scenario("_");
			var activity = new Activity("_") { InWorkTime = true };
			var dateOnly = DateOnly.Today;
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(15, 0, 15, 0, 60), new TimePeriodWithSegment(23, 0, 23, 0, 60), new ShiftCategory("_").WithId()));
			var contract = new Contract("_") { WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(1), TimeSpan.FromHours(83), TimeSpan.FromHours(11), TimeSpan.FromHours(16)) };
			var definitionSet = new MultiplicatorDefinitionSet("overtime", MultiplicatorType.Overtime);
			contract.AddMultiplicatorDefinitionSetCollection(definitionSet);
			var skill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
			var skillDay = skill.CreateSkillDayWithDemand(scenario, dateOnly, TimeSpan.FromMinutes(60));
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(contract, skill).WithSchedulePeriodOneDay(dateOnly);
			var personAssignment = new PersonAssignment(agent, scenario, dateOnly.AddDays(1));
			personAssignment.AddActivity(activity, new DateOnlyAsDateTimePeriod(dateOnly.AddDays(1), agent.PermissionInformation.DefaultTimeZone()).Period());
			personAssignment.SetShiftCategory(new ShiftCategory("_"));
			var overtimePreference = new OvertimePreferences {AllowBreakNightlyRest = false, OvertimeType = definitionSet, ShiftBagToUse = new RuleSetBag(ruleSet), ScheduleTag = new ScheduleTag() };
			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, new DateOnlyPeriod(dateOnly, dateOnly), new[] { agent }, new[] { personAssignment }, skillDay);

			Target.Execute(overtimePreference, new NoSchedulingProgress(), new[] { stateHolder.Schedules[agent].ScheduledDay(dateOnly) });

			stateHolder.Schedules[agent].ScheduledDay(dateOnly).PersonAssignment(true).OvertimeActivities()
				.Should().Be.Empty();
		}


		[Test]
		public void ShouldConsiderNightlyRestWhenOvertimeIsPresent()
		{
			var scenario = new Scenario("_");
			var activity = new Activity("_") { InWorkTime = true };
			var dateOnly = DateOnly.Today;
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(15, 0, 15, 0, 60), new TimePeriodWithSegment(23, 0, 23, 0, 60), new ShiftCategory("_").WithId()));
			var contract = new Contract("_") { WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(1), TimeSpan.FromHours(83), TimeSpan.FromHours(11), TimeSpan.FromHours(16)) };
			var definitionSet = new MultiplicatorDefinitionSet("overtime", MultiplicatorType.Overtime);
			contract.AddMultiplicatorDefinitionSetCollection(definitionSet);
			var skill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
			var skillDay = skill.CreateSkillDayWithDemand(scenario, dateOnly, TimeSpan.FromMinutes(60));
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(contract, skill).WithSchedulePeriodOneDay(dateOnly);
			var personAssignment = new PersonAssignment(agent, scenario, dateOnly.AddDays(1));
			personAssignment.AddOvertimeActivity(activity, new DateOnlyAsDateTimePeriod(dateOnly.AddDays(1), agent.PermissionInformation.DefaultTimeZone()).Period(), new MultiplicatorDefinitionSet("_", MultiplicatorType.Overtime));
			var overtimePreference = new OvertimePreferences { AllowBreakNightlyRest = false, OvertimeType = definitionSet, ShiftBagToUse = new RuleSetBag(ruleSet), ScheduleTag = new ScheduleTag() };
			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, new DateOnlyPeriod(dateOnly, dateOnly), new[] { agent }, new[] { personAssignment }, skillDay);

			Target.Execute(overtimePreference, new NoSchedulingProgress(), new[] { stateHolder.Schedules[agent].ScheduledDay(dateOnly) });

			stateHolder.Schedules[agent].ScheduledDay(dateOnly).PersonAssignment(true).OvertimeActivities()
				.Should().Be.Empty();
		}

		[Test]
		public void ShouldConsiderWeeklyRest()
		{
			var scenario = new Scenario("_");
			var activity = new Activity("_") { InWorkTime = true };
			var dateOnly = new DateOnly(2016, 04, 15);
			var dateToSchedule = new DateOnly(2016, 04, 20);
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(7, 0, 8, 0, 60), new TimePeriodWithSegment(15, 0, 16, 0, 60), new ShiftCategory("_").WithId()));
			var contract = new Contract("_") { WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(1), TimeSpan.FromHours(83), TimeSpan.FromHours(1), TimeSpan.FromHours(38)) };
			var definitionSet = new MultiplicatorDefinitionSet("overtime", MultiplicatorType.Overtime);
			contract.AddMultiplicatorDefinitionSetCollection(definitionSet);
			var skill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
			var skillDay = skill.CreateSkillDayWithDemand(scenario, dateToSchedule, TimeSpan.FromMinutes(60));
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(contract, skill);
			agent.AddSchedulePeriod(new SchedulePeriod(dateOnly, SchedulePeriodType.Week, 4));
			var overtimePreference = new OvertimePreferences { AllowBreakWeeklyRest = false, OvertimeType = definitionSet, ShiftBagToUse = new RuleSetBag(ruleSet), ScheduleTag = new ScheduleTag() };

			var personAssignments = new List<IPersonAssignment>();
			for (var i = 0; i < 15; i++)
			{
				var date = dateOnly.AddDays(i);
				if (date == dateToSchedule) continue;
				var personAss = new PersonAssignment(agent, scenario, date);
				var period = new DateTimePeriod(date.Year, date.Month, date.Day, 8, date.Year, date.Month, date.Day, 16);
				personAss.AddActivity(activity, period);
				personAssignments.Add(personAss);
			}

			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, new DateOnlyPeriod(dateOnly, dateOnly.AddDays(15)), new[] { agent }, personAssignments, skillDay);

			Target.Execute(overtimePreference, new NoSchedulingProgress(), new[] { stateHolder.Schedules[agent].ScheduledDay(dateToSchedule) });

			stateHolder.Schedules[agent].ScheduledDay(dateToSchedule).PersonAssignment(true).OvertimeActivities()
				.Should().Be.Empty();
		}

		[Test]
		public void ShouldNotConsiderWeeklyRest()
		{
			var scenario = new Scenario("_");
			var activity = new Activity("_") { InWorkTime = true };
			var dateOnly = new DateOnly(2016, 04, 15);
			var dateToSchedule = new DateOnly(2016, 04, 20);
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(7, 0, 8, 0, 60), new TimePeriodWithSegment(15, 0, 16, 0, 60), new ShiftCategory("_").WithId()));
			var contract = new Contract("_") { WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(1), TimeSpan.FromHours(83), TimeSpan.FromHours(1), TimeSpan.FromHours(38)) };
			var definitionSet = new MultiplicatorDefinitionSet("overtime", MultiplicatorType.Overtime);
			contract.AddMultiplicatorDefinitionSetCollection(definitionSet);
			var skill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
			var skillDay = skill.CreateSkillDayWithDemand(scenario, dateToSchedule, TimeSpan.FromMinutes(60));
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(contract, skill);
			agent.AddSchedulePeriod(new SchedulePeriod(dateOnly, SchedulePeriodType.Week, 4));
			var overtimePreference = new OvertimePreferences { AllowBreakWeeklyRest = true, OvertimeType = definitionSet, ShiftBagToUse = new RuleSetBag(ruleSet), ScheduleTag = new ScheduleTag() };

			var personAssignments = new List<IPersonAssignment>();
			for (var i = 0; i < 15; i++)
			{
				var date = dateOnly.AddDays(i);
				if (date == dateToSchedule) continue;
				var personAss = new PersonAssignment(agent, scenario, date);
				var period = new DateTimePeriod(date.Year, date.Month, date.Day, 8, date.Year, date.Month, date.Day, 16);
				personAss.AddActivity(activity, period);
				personAssignments.Add(personAss);
			}

			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, new DateOnlyPeriod(dateOnly, dateOnly.AddDays(15)), new[] { agent }, personAssignments, skillDay);

			Target.Execute(overtimePreference, new NoSchedulingProgress(), new[] { stateHolder.Schedules[agent].ScheduledDay(dateToSchedule) });

			stateHolder.Schedules[agent].ScheduledDay(dateToSchedule).PersonAssignment(true).OvertimeActivities()
				.Should().Not.Be.Empty();
		}

		[Test]
		public void ShouldNotPlaceOvertimeWhenNoAvailabilityAndOnlyAvailableAgents()
		{
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var dateOnly = DateOnly.Today;
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(7, 0, 8, 0, 60), new TimePeriodWithSegment(15, 0, 16, 0, 60), new ShiftCategory("_").WithId()));
			var contract = new Contract("_") { WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(10), TimeSpan.FromHours(83), TimeSpan.FromHours(1), TimeSpan.FromHours(16)) };
			var definitionSet = new MultiplicatorDefinitionSet("overtime", MultiplicatorType.Overtime);
			contract.AddMultiplicatorDefinitionSetCollection(definitionSet);
			var skill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
			var skillDay = skill.CreateSkillDayWithDemand(scenario, dateOnly, TimeSpan.FromMinutes(60));
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(contract, skill).WithSchedulePeriodOneDay(dateOnly);
			var overtimePreference = new OvertimePreferences {AvailableAgentsOnly = true, OvertimeType = definitionSet, ShiftBagToUse = new RuleSetBag(ruleSet), ScheduleTag = new ScheduleTag() };
			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, new DateOnlyPeriod(dateOnly, dateOnly), new[] { agent }, Enumerable.Empty<IScheduleData>(), skillDay);

			Target.Execute(overtimePreference, new NoSchedulingProgress(), new[] { stateHolder.Schedules[agent].ScheduledDay(dateOnly) });

			stateHolder.Schedules[agent].ScheduledDay(dateOnly).PersonAssignment(true).OvertimeActivities()
				.Should().Be.Empty();
		}

		[Test]
		public void ShouldPlaceOvertimeWhenAgentHasAvailabilityAndOnlyAvailableAgents()
		{
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var dateOnly = DateOnly.Today;
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(7, 0, 8, 0, 60), new TimePeriodWithSegment(15, 0, 16, 0, 60), new ShiftCategory("_").WithId()));
			var contract = new Contract("_") { WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(10), TimeSpan.FromHours(83), TimeSpan.FromHours(1), TimeSpan.FromHours(16)) };
			var definitionSet = new MultiplicatorDefinitionSet("overtime", MultiplicatorType.Overtime);
			contract.AddMultiplicatorDefinitionSetCollection(definitionSet);
			var skill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
			var skillDay = skill.CreateSkillDayWithDemand(scenario, dateOnly, TimeSpan.FromMinutes(60));
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(contract, skill).WithSchedulePeriodOneDay(dateOnly);
			var overtimeAvailability = new OvertimeAvailability(agent, dateOnly,TimeSpan.FromHours(5), TimeSpan.FromHours(20));
			var overtimePreference = new OvertimePreferences { AvailableAgentsOnly = true, OvertimeType = definitionSet, ShiftBagToUse = new RuleSetBag(ruleSet), ScheduleTag = new ScheduleTag() };
			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, new DateOnlyPeriod(dateOnly, dateOnly), new[] { agent }, new [] {overtimeAvailability}, skillDay);

			Target.Execute(overtimePreference, new NoSchedulingProgress(), new[] { stateHolder.Schedules[agent].ScheduledDay(dateOnly) });

			stateHolder.Schedules[agent].ScheduledDay(dateOnly).PersonAssignment(true).OvertimeActivities()
				.Should().Not.Be.Empty();
		}

		[Test]
		public void ShouldNotPlaceOvertimeWhenAgentHasAvailabilityNotCoveringShiftAndOnlyAvailableAgents()
		{
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var dateOnly = DateOnly.Today;
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(7, 0, 8, 0, 60), new TimePeriodWithSegment(15, 0, 16, 0, 60), new ShiftCategory("_").WithId()));
			var contract = new Contract("_") { WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(10), TimeSpan.FromHours(83), TimeSpan.FromHours(1), TimeSpan.FromHours(16)) };
			var definitionSet = new MultiplicatorDefinitionSet("overtime", MultiplicatorType.Overtime);
			contract.AddMultiplicatorDefinitionSetCollection(definitionSet);
			var skill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
			var skillDay = skill.CreateSkillDayWithDemand(scenario, dateOnly, TimeSpan.FromMinutes(60));
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(contract, skill).WithSchedulePeriodOneDay(dateOnly);
			var overtimeAvailability = new OvertimeAvailability(agent, dateOnly, TimeSpan.FromHours(12), TimeSpan.FromHours(20));
			var overtimePreference = new OvertimePreferences { AvailableAgentsOnly = true, OvertimeType = definitionSet, ShiftBagToUse = new RuleSetBag(ruleSet), ScheduleTag = new ScheduleTag() };
			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, new DateOnlyPeriod(dateOnly, dateOnly), new[] { agent }, new[] { overtimeAvailability }, skillDay);

			Target.Execute(overtimePreference, new NoSchedulingProgress(), new[] { stateHolder.Schedules[agent].ScheduledDay(dateOnly) });

			stateHolder.Schedules[agent].ScheduledDay(dateOnly).PersonAssignment(true).OvertimeActivities()
				.Should().Be.Empty();
		}

		[Test]
		public void ShouldNotConsiderOvertimeAvailability()
		{
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var dateOnly = DateOnly.Today;
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(7, 0, 8, 0, 60), new TimePeriodWithSegment(15, 0, 16, 0, 60), new ShiftCategory("_").WithId()));
			var contract = new Contract("_") { WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(10), TimeSpan.FromHours(83), TimeSpan.FromHours(1), TimeSpan.FromHours(16)) };
			var definitionSet = new MultiplicatorDefinitionSet("overtime", MultiplicatorType.Overtime);
			contract.AddMultiplicatorDefinitionSetCollection(definitionSet);
			var skill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
			var skillDay = skill.CreateSkillDayWithDemand(scenario, dateOnly, TimeSpan.FromMinutes(60));
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(contract, skill).WithSchedulePeriodOneDay(dateOnly);
			var overtimePreference = new OvertimePreferences { AvailableAgentsOnly = false, OvertimeType = definitionSet, ShiftBagToUse = new RuleSetBag(ruleSet), ScheduleTag = new ScheduleTag() };
			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, new DateOnlyPeriod(dateOnly, dateOnly), new[] { agent }, Enumerable.Empty<IScheduleData>(), skillDay);

			Target.Execute(overtimePreference, new NoSchedulingProgress(), new[] { stateHolder.Schedules[agent].ScheduledDay(dateOnly) });

			stateHolder.Schedules[agent].ScheduledDay(dateOnly).PersonAssignment(true).OvertimeActivities()
				.Should().Not.Be.Empty();
		}

		[Test]
		public void ShouldNotPlaceOvertimeWhenResultGetsWorse()
		{
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var dateOnly = DateOnly.Today;
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(7, 0, 8, 0, 60), new TimePeriodWithSegment(15, 0, 16, 0, 60), new ShiftCategory("_").WithId()));
			var contract = new Contract("_") { WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(10), TimeSpan.FromHours(83), TimeSpan.FromHours(1), TimeSpan.FromHours(16)) };
			var definitionSet = new MultiplicatorDefinitionSet("overtime", MultiplicatorType.Overtime);
			contract.AddMultiplicatorDefinitionSetCollection(definitionSet);
			var skill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
			var skillDay = skill.CreateSkillDayWithDemand(scenario, dateOnly, TimeSpan.FromMinutes(10));
			var scheduledAgent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(contract, skill).WithSchedulePeriodOneDay(dateOnly);
			var agentToSchedule = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(contract, skill).WithSchedulePeriodOneDay(dateOnly);
			var overtimePreference = new OvertimePreferences
			{
				OvertimeType = definitionSet,
				ShiftBagToUse = new RuleSetBag(ruleSet),
				ScheduleTag = new ScheduleTag(),
				SelectedTimePeriod = new TimePeriod(0, 0, 0, 0) //to prevent overtime isn't added at beginning and ending of shift
			};
			var ass = new PersonAssignment(scheduledAgent, scenario, dateOnly);
			ass.AddActivity(activity, new DateOnlyPeriod(dateOnly, dateOnly).ToDateTimePeriod(scheduledAgent.PermissionInformation.DefaultTimeZone()));
			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, new DateOnlyPeriod(dateOnly, dateOnly), new[] { scheduledAgent, agentToSchedule }, new[] { ass }, skillDay);

			Target.Execute(overtimePreference, new NoSchedulingProgress(), new[] { stateHolder.Schedules[agentToSchedule].ScheduledDay(dateOnly) });

			stateHolder.Schedules[agentToSchedule].ScheduledDay(dateOnly).PersonAssignment(true).OvertimeActivities()
				.Should().Be.Empty();
		}

		[Test]
		public void ShouldWorkWithDifferentActivityInstancesWithSameId_WhenItShouldPlaceShift()
		{
			var activityId = Guid.NewGuid();
			var scenario = new Scenario("_");
			var dateOnly = DateOnly.Today;
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(new Activity("_") { RequiresSkill = true }.WithId(activityId), new TimePeriodWithSegment(7, 0, 8, 0, 60), new TimePeriodWithSegment(15, 0, 16, 0, 60), new ShiftCategory("_").WithId()));
			var contract = new Contract("_") { WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(10), TimeSpan.FromHours(83), TimeSpan.FromHours(1), TimeSpan.FromHours(16)) };
			var definitionSet = new MultiplicatorDefinitionSet("overtime", MultiplicatorType.Overtime);
			contract.AddMultiplicatorDefinitionSetCollection(definitionSet);
			var skill = new Skill("_").For(new Activity("_") { RequiresSkill = true }.WithId(activityId)).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
			var skillDay = skill.CreateSkillDayWithDemand(scenario, dateOnly, TimeSpan.FromMinutes(60));
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(contract, skill).WithSchedulePeriodOneDay(dateOnly);
			var overtimePreference = new OvertimePreferences { OvertimeType = definitionSet, ShiftBagToUse = new RuleSetBag(ruleSet), ScheduleTag = new ScheduleTag() };
			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, new DateOnlyPeriod(dateOnly, dateOnly), new[] { agent }, Enumerable.Empty<IScheduleData>(), skillDay);

			Target.Execute(overtimePreference, new NoSchedulingProgress(), new[] { stateHolder.Schedules[agent].ScheduledDay(dateOnly) });

			stateHolder.Schedules[agent].ScheduledDay(dateOnly).PersonAssignment(true).OvertimeActivities()
				.Should().Not.Be.Empty();
		}

		[Test]
		public void ShouldWorkWithDifferentActivityInstancesWithSameId_WhenItShouldNotPlaceShift()
		{
			var activityId = Guid.NewGuid();
			var scenario = new Scenario("_");
			var dateOnly = DateOnly.Today;
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(new Activity("_").WithId(activityId), new TimePeriodWithSegment(7, 0, 8, 0, 60), new TimePeriodWithSegment(15, 0, 16, 0, 60), new ShiftCategory("_").WithId()));
			var contract = new Contract("_") { WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(10), TimeSpan.FromHours(83), TimeSpan.FromHours(1), TimeSpan.FromHours(16)) };
			var definitionSet = new MultiplicatorDefinitionSet("overtime", MultiplicatorType.Overtime);
			contract.AddMultiplicatorDefinitionSetCollection(definitionSet);
			var skill = new Skill("_").For(new Activity("_").WithId(activityId)).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
			var skillDay = skill.CreateSkillDayWithDemand(scenario, dateOnly, TimeSpan.FromMinutes(10));
			var scheduledAgent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(contract, skill).WithSchedulePeriodOneDay(dateOnly);
			var agentToSchedule = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(contract, skill).WithSchedulePeriodOneDay(dateOnly);
			var overtimePreference = new OvertimePreferences
			{
				OvertimeType = definitionSet,
				ShiftBagToUse = new RuleSetBag(ruleSet),
				ScheduleTag = new ScheduleTag(),
				SelectedTimePeriod = new TimePeriod(0, 0, 0, 0) //to prevent overtime isn't added at beginning and ending of shift
			};
			var ass = new PersonAssignment(scheduledAgent, scenario, dateOnly);
			ass.AddActivity(new Activity("_").WithId(activityId), new DateOnlyPeriod(dateOnly, dateOnly).ToDateTimePeriod(scheduledAgent.PermissionInformation.DefaultTimeZone()));
			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, new DateOnlyPeriod(dateOnly, dateOnly), new[] { scheduledAgent, agentToSchedule }, new[] { ass }, skillDay);

			Target.Execute(overtimePreference, new NoSchedulingProgress(), new[] { stateHolder.Schedules[agentToSchedule].ScheduledDay(dateOnly) });

			stateHolder.Schedules[agentToSchedule].ScheduledDay(dateOnly).PersonAssignment(true).OvertimeActivities()
				.Should().Be.Empty();
		}


		[Test]
		public void ShouldNotConsideredMaxContractTime()
		{
			var scenario = new Scenario("_");
			var activity = new Activity("_"){ InWorkTime = true };
			var dateOnly = DateOnly.Today;
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(7, 0, 8, 0, 60), new TimePeriodWithSegment(15, 0, 16, 0, 60), new ShiftCategory("_").WithId()));
			var contract = new Contract("_") { WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(0), TimeSpan.FromHours(100), TimeSpan.FromHours(1), TimeSpan.FromHours(1)) };
			var definitionSet = new MultiplicatorDefinitionSet("overtime", MultiplicatorType.Overtime);
			contract.AddMultiplicatorDefinitionSetCollection(definitionSet);
			var skill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
			var skillDay = skill.CreateSkillDayWithDemand(scenario, dateOnly, TimeSpan.FromMinutes(60));
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(contract, skill).WithSchedulePeriodOneDay(dateOnly.AddDays(-10));
			var overtimePreference = new OvertimePreferences {OvertimeType = definitionSet, ShiftBagToUse = new RuleSetBag(ruleSet), ScheduleTag = new ScheduleTag()};
			var ass = new PersonAssignment(agent, scenario, dateOnly);
			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, DateOnlyPeriod.CreateWithNumberOfWeeks(dateOnly, 1), new[] { agent }, new[] { ass }, skillDay);

			Target.Execute(overtimePreference, new NoSchedulingProgress(), new[] { stateHolder.Schedules[agent].ScheduledDay(dateOnly) });

			stateHolder.Schedules[agent].ScheduledDay(dateOnly).PersonAssignment(true).OvertimeActivities()
				.Should().Not.Be.Empty();
		}

		[Test]
		public void ShouldNotCareAboutSchedulersUseValidationSettingWhenCreatingRules()
		{
			var scenario = new Scenario("_");
			var activity = new Activity("_") { InWorkTime = true };
			var dateOnly = DateOnly.Today;
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(7, 0, 8, 0, 60), new TimePeriodWithSegment(15, 0, 16, 0, 60), new ShiftCategory("_").WithId()));
			var contract = new Contract("_") { WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(1), TimeSpan.FromHours(1), TimeSpan.FromHours(1), TimeSpan.FromHours(16)) };
			var definitionSet = new MultiplicatorDefinitionSet("overtime", MultiplicatorType.Overtime);
			contract.AddMultiplicatorDefinitionSetCollection(definitionSet);
			var skill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
			var skillDay = skill.CreateSkillDayWithDemand(scenario, dateOnly, TimeSpan.FromMinutes(60));
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(contract, skill).WithSchedulePeriodOneDay(dateOnly);
			var overtimePreference = new OvertimePreferences { AllowBreakMaxWorkPerWeek = false, OvertimeType = definitionSet, ShiftBagToUse = new RuleSetBag(ruleSet), ScheduleTag = new ScheduleTag() };
			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, new DateOnlyPeriod(dateOnly, dateOnly), new[] { agent }, Enumerable.Empty<IScheduleData>(), skillDay);
			stateHolder.SchedulingResultState.UseValidation = true;

			Target.Execute(overtimePreference, new NoSchedulingProgress(), new[] { stateHolder.Schedules[agent].ScheduledDay(dateOnly) });

			stateHolder.Schedules[agent].ScheduledDay(dateOnly).PersonAssignment(true).OvertimeActivities()
				.Should().Be.Empty();
		}

		[Test]
		public void ShouldNotUseAvarageShiftLengths()
		{	
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var dateOnly = DateOnly.Today;
			var lowAverageShiftLength = TimeSpan.FromHours(4);
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(7, 0, 12, 0, 300), new TimePeriodWithSegment(16, 0, 16, 0, 60), new ShiftCategory("_").WithId()));
			var contract = new Contract("_") {WorkTimeDirective =  new WorkTimeDirective(TimeSpan.FromHours(10), TimeSpan.FromHours(83), TimeSpan.FromHours(1), TimeSpan.FromHours(16)),
				WorkTime = new WorkTime(lowAverageShiftLength)};
			var definitionSet = new MultiplicatorDefinitionSet("overtime", MultiplicatorType.Overtime);
			contract.AddMultiplicatorDefinitionSetCollection(definitionSet);
			var skill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
			var skillDay = skill.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(contract, skill).WithSchedulePeriodOneDay(dateOnly);
			var overtimePreference = new OvertimePreferences { OvertimeType = definitionSet, ShiftBagToUse = new RuleSetBag(ruleSet), ScheduleTag = new ScheduleTag() };
			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, new DateOnlyPeriod(dateOnly, dateOnly), new[] { agent }, Enumerable.Empty<IScheduleData>(), skillDay);

			Target.Execute(overtimePreference, new NoSchedulingProgress(), new[] { stateHolder.Schedules[agent].ScheduledDay(dateOnly) });

			stateHolder.Schedules[agent].ScheduledDay(dateOnly).PersonAssignment().OvertimeActivities().Single().Period.ElapsedTime()
				.Should().Be.EqualTo(TimeSpan.FromHours(9));
		}

		[Test]
		public void ShouldPlaceOvertimeOnEmptyDayWithDemandNotCaringAboutSchedulingRestrictionsBug41536()
		{
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var dateOnly = DateOnly.Today;
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(7, 0, 8, 0, 60), new TimePeriodWithSegment(15, 0, 16, 0, 60), new ShiftCategory("_").WithId()));
			var contract = new Contract("_") { WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(10), TimeSpan.FromHours(83), TimeSpan.FromHours(1), TimeSpan.FromHours(16)) };
			var definitionSet = new MultiplicatorDefinitionSet("overtime", MultiplicatorType.Overtime);
			contract.AddMultiplicatorDefinitionSetCollection(definitionSet);
			var skill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
			var skillDay = skill.CreateSkillDayWithDemand(scenario, dateOnly, TimeSpan.FromMinutes(60));
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(contract, skill).WithSchedulePeriodOneDay(dateOnly);
			var workTimeLimitation = new WorkTimeLimitation(TimeSpan.FromHours(1), TimeSpan.FromHours(1));
			var preference = new ScheduleDataRestriction(agent, new	PreferenceRestriction {WorkTimeLimitation = workTimeLimitation },dateOnly);
			var rotation = new ScheduleDataRestriction(agent, new RotationRestriction {WorkTimeLimitation = workTimeLimitation},
				dateOnly);
			var availability = new ScheduleDataRestriction(agent,
				new AvailabilityRestriction
				{
					StartTimeLimitation = new StartTimeLimitation(TimeSpan.FromHours(1), null),
					EndTimeLimitation = new EndTimeLimitation(null, TimeSpan.FromHours(2))
				}, dateOnly);
			var overtimePreference = new OvertimePreferences { OvertimeType = definitionSet, ShiftBagToUse = new RuleSetBag(ruleSet), ScheduleTag = new ScheduleTag() };
			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, new DateOnlyPeriod(dateOnly, dateOnly), new[] { agent }, new[] { preference, rotation, availability }, skillDay);

			Target.Execute(overtimePreference, new NoSchedulingProgress(), new[] { stateHolder.Schedules[agent].ScheduledDay(dateOnly) });

			stateHolder.Schedules[agent].ScheduledDay(dateOnly).PersonAssignment(true).OvertimeActivities()
				.Should().Not.Be.Empty();
		}

		[Test]
		public void ShouldNotCrashWhenAgentHasNoPersonPeriod()
		{
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var dateOnly = DateOnly.Today;
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(7, 0, 8, 0, 60), new TimePeriodWithSegment(15, 0, 16, 0, 60), new ShiftCategory("_").WithId()));
			var definitionSet = new MultiplicatorDefinitionSet("overtime", MultiplicatorType.Overtime);
			var skill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
			var skillDay = skill.CreateSkillDayWithDemand(scenario, dateOnly, TimeSpan.FromMinutes(60));
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithSchedulePeriodOneDay(dateOnly);
			var overtimePreference = new OvertimePreferences { OvertimeType = definitionSet, ShiftBagToUse = new RuleSetBag(ruleSet), ScheduleTag = new ScheduleTag() };
			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, new DateOnlyPeriod(dateOnly, dateOnly), new[] { agent }, Enumerable.Empty<IScheduleData>(), skillDay);

			Assert.DoesNotThrow(() =>
			{
				Target.Execute(overtimePreference, new NoSchedulingProgress(), new[] { stateHolder.Schedules[agent].ScheduledDay(dateOnly) });
			});
		}

		[Test]
		public void ShouldNotCrashWhenAgentHasNoContract()
		{
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var dateOnly = DateOnly.Today;
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(7, 0, 8, 0, 60), new TimePeriodWithSegment(15, 0, 16, 0, 60), new ShiftCategory("_").WithId()));
			var definitionSet = new MultiplicatorDefinitionSet("overtime", MultiplicatorType.Overtime);
			var skill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
			var skillDay = skill.CreateSkillDayWithDemand(scenario, dateOnly, TimeSpan.FromMinutes(60));
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(skill).WithSchedulePeriodOneDay(dateOnly);
			agent.Period(dateOnly).PersonContract = null;
			var overtimePreference = new OvertimePreferences { OvertimeType = definitionSet, ShiftBagToUse = new RuleSetBag(ruleSet), ScheduleTag = new ScheduleTag() };
			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, new DateOnlyPeriod(dateOnly, dateOnly), new[] { agent }, Enumerable.Empty<IScheduleData>(), skillDay);

			Assert.DoesNotThrow(() =>
			{
				Target.Execute(overtimePreference, new NoSchedulingProgress(), new[] { stateHolder.Schedules[agent].ScheduledDay(dateOnly) });
			});
		}

		[Test]
		public void ShouldNotCrashWhenAgentDoesntHaveSchedulePeriod()
		{
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var dateOnly = DateOnly.Today;
			var contract = new ContractWithMaximumTolerance();
			var definitionSet = new MultiplicatorDefinitionSet("overtime", MultiplicatorType.Overtime);
			contract.AddMultiplicatorDefinitionSetCollection(definitionSet);
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(7, 0, 8, 0, 60), new TimePeriodWithSegment(15, 0, 16, 0, 60), new ShiftCategory("_").WithId()));
			var skill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
			var skillDay = skill.CreateSkillDayWithDemand(scenario, dateOnly, TimeSpan.FromMinutes(60));
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(contract, skill);
			var overtimePreference = new OvertimePreferences { OvertimeType = definitionSet, ShiftBagToUse = new RuleSetBag(ruleSet), ScheduleTag = new ScheduleTag() };
			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, new DateOnlyPeriod(dateOnly, dateOnly), new[] { agent }, Enumerable.Empty<IScheduleData>(), skillDay);

			Assert.DoesNotThrow(() =>
			{
				Target.Execute(overtimePreference, new NoSchedulingProgress(), new[] { stateHolder.Schedules[agent].ScheduledDay(dateOnly) });
			});
		}

		[TestCase(false, ExpectedResult = false)]
		[TestCase(true, ExpectedResult = true)]
		public bool ShouldNotAddOvertimeWhenNotPossibleToGetEffectiveRestriction(bool onlyAvailable)
		{
			var scenario = new Scenario();
			var activity = new Activity();
			var dateOnly = DateOnly.Today;
			var shiftCategory = new ShiftCategory().WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 60), new TimePeriodWithSegment(16, 0, 16, 0, 60), shiftCategory));
			var contract = new Contract("_") { WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(10), TimeSpan.FromHours(83), TimeSpan.FromHours(11), TimeSpan.FromHours(16)) };
			var definitionSet = new MultiplicatorDefinitionSet("_", MultiplicatorType.Overtime);
			contract.AddMultiplicatorDefinitionSetCollection(definitionSet);
			var skill = new Skill().For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
			var skillDay = skill.CreateSkillDayWithDemand(scenario, dateOnly, TimeSpan.FromMinutes(60));
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet, contract, skill).WithSchedulePeriodOneDay(dateOnly);
			var assDayBefore = new PersonAssignment(agent, scenario, dateOnly.AddDays(-1)).WithLayer(activity, new TimePeriod(10, 19)).ShiftCategory(shiftCategory);
			var overtimeAvailability = new OvertimeAvailability(agent, dateOnly, TimeSpan.FromHours(4), TimeSpan.FromHours(5));
			var overtimePreference = new OvertimePreferences
			{
				OvertimeType = definitionSet,
				ShiftBagToUse = new RuleSetBag(ruleSet),
				ScheduleTag = new ScheduleTag(),
				AvailableAgentsOnly = onlyAvailable,
				SelectedSpecificTimePeriod = new TimePeriod(TimeSpan.FromHours(0), TimeSpan.FromHours(34)),
				SelectedTimePeriod = new TimePeriod(TimeSpan.FromHours(1), TimeSpan.FromHours(1))
			};
			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, new DateOnlyPeriod(dateOnly.AddDays(-1), dateOnly), new[] { agent }, new IPersistableScheduleData[] { assDayBefore, overtimeAvailability }, skillDay);

			Target.Execute(overtimePreference, new NoSchedulingProgress(), new[] { stateHolder.Schedules[agent].ScheduledDay(dateOnly) });

			return stateHolder.Schedules[agent].ScheduledDay(dateOnly).PersonAssignment(true).OvertimeActivities().IsEmpty();
		}
	}
}