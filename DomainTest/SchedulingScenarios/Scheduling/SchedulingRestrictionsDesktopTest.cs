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
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.Scheduling
{
	[DomainTest]
	[UseIocForFatClient]
	public class SchedulingRestrictionsDesktopTest : SchedulingScenario
	{
		public DesktopScheduling Target;
		public Func<ISchedulerStateHolder> SchedulerStateHolderFrom;
		public FakeBusinessUnitRepository BusinessUnitRepository;

		[Test]
		public void ShouldNotScheduleShiftsForRestrictionsOnlyWhenNoRestrictionExists()
		{
			BusinessUnitRepository.Has(ServiceLocatorForEntity.CurrentBusinessUnit.Current());
			var date = new DateOnly(2017, 1, 10);
			var activity = new Activity("_").WithId();
			var otherActivity = new Activity("_").WithId();
			otherActivity.RequiresSkill = true;
			var scenario = new Scenario("_");
			var skill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
			var skillDay = skill.CreateSkillDayWithDemand(scenario, date, 100);
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), new ShiftCategory("_").WithId())) { OnlyForRestrictions = true };
			var rulsetToGetMinMaxWorkTimeFrom = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(otherActivity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), new ShiftCategory("_").WithId())); //so we get min max worktime from the ruleset bag when not having any availability restriction
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet, skill).WithSchedulePeriodOneDay(date);
			agent.Period(date).RuleSetBag.AddRuleSet(rulsetToGetMinMaxWorkTimeFrom);
			var schedulerStateHolder = SchedulerStateHolderFrom.Fill(scenario, new DateOnlyPeriod(date, date), new[] { agent }, Enumerable.Empty<IPersonAssignment>(), new[] { skillDay });
			var schedulingOptions = new SchedulingOptions { UseAvailability = true };

			Target.Execute(new NoSchedulingCallback(),
				schedulingOptions,
				new NoSchedulingProgress(),
				schedulerStateHolder.SchedulingResultState.LoadedAgents.FixedStaffPeople(date.ToDateOnlyPeriod()), date.ToDateOnlyPeriod()
				);

			schedulerStateHolder.Schedules[agent].ScheduledDay(date).IsScheduled().Should().Be.False();
		}
		
		[Test]
		public void ShouldScheduleAbsencePreferences()
		{
			var date = new DateOnly(2017, 1, 10);
			var activity = new Activity().WithId();
			var scenario = new Scenario();
			var skill = new Skill().For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
			var skillDay = skill.CreateSkillDayWithDemand(scenario, date, 100);
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), new ShiftCategory("_").WithId()));
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet, skill).WithSchedulePeriodOneDay(date);
			var absenceRestriction = new PreferenceRestriction {Absence = new Absence().WithId()};
			var preferenceDay = new PreferenceDay(agent, date, absenceRestriction).WithId();
			var schedulerStateHolder = SchedulerStateHolderFrom.Fill(scenario, date, agent, preferenceDay,skillDay);
			var schedulingOptions = new SchedulingOptions { UsePreferences = true };

			Target.Execute(new NoSchedulingCallback(), schedulingOptions, new NoSchedulingProgress(), new[] {agent}, date.ToDateOnlyPeriod());

			schedulerStateHolder.Schedules[agent].ScheduledDay(date).PersonAbsenceCollection().Count
				.Should().Be.EqualTo(1);
		}
		
		[Test]
		public void ShouldScheduleFixedStaffWhenUsingHourlyAvailability()
		{
			var firstDay = new DateOnly(2017, 5, 15);
			var period = DateOnlyPeriod.CreateWithNumberOfWeeks(firstDay, 1);
			var activity = new Activity().WithId();
			var skill = new Skill().For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
			var scenario = new Scenario();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), new ShiftCategory("_").WithId()));
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet, skill).WithSchedulePeriodOneWeek(firstDay);
			var hourlyAvailabilityDays = new List<IStudentAvailabilityDay>();
			for (var i = 0; i < 4; i++)
			{
				hourlyAvailabilityDays.Add(new StudentAvailabilityDay(agent, firstDay.AddDays(i), new[]
				{
					new StudentAvailabilityRestriction {
						StartTimeLimitation = new StartTimeLimitation(new TimeSpan(4, 0, 0), null),
						EndTimeLimitation = new EndTimeLimitation(null, new TimeSpan(21, 0, 0))
					}
				}));
			}
			var skillDays = skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay, 10, 10, 10, 10, 10, 10, 10);
			var schedulerStateHolder = SchedulerStateHolderFrom.Fill(scenario, period, agent, hourlyAvailabilityDays, skillDays);
			var schedulingOptions = new SchedulingOptions { ScheduleEmploymentType = ScheduleEmploymentType.FixedStaff, UseStudentAvailability = true };
			
			Target.Execute(new NoSchedulingCallback(), schedulingOptions, new NoSchedulingProgress(), new[] { agent }, period);

			for (var i = 0; i < 7; i++)
			{
				if (i < 4)
					schedulerStateHolder.Schedules[agent].ScheduledDay(firstDay.AddDays(i)).PersonAssignment(true).ShiftLayers.Should().Not.Be.Empty();
				else
					schedulerStateHolder.Schedules[agent].ScheduledDay(firstDay.AddDays(i)).HasDayOff().Should().Be.True();
			}	
		}

		public SchedulingRestrictionsDesktopTest(SeperateWebRequest seperateWebRequest, RemoveClassicShiftCategory resourcePlannerRemoveClassicShiftCat46582, RemoveImplicitResCalcContext removeImplicitResCalcContext46680, bool resourcePlannerBpoScheduling46265) : base(seperateWebRequest, resourcePlannerRemoveClassicShiftCat46582, removeImplicitResCalcContext46680, resourcePlannerBpoScheduling46265)
		{
		}
	}
}
