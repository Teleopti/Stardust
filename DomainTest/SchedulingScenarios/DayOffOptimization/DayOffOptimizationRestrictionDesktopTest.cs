using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
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
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Scheduling;


namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.DayOffOptimization
{
	[DomainTest]
	[UseIocForFatClient]
	[UseEventPublisher(typeof(SyncInFatClientProcessEventPublisher))]
	public class DayOffOptimizationRestrictionDesktopTest : DayOffOptimizationScenario
	{
		public Func<ISchedulerStateHolder> SchedulerStateHolder;
		public DayOffOptimizationDesktop Target;

		[Test]
		public void ShouldMoveDaysOffWhenUsingHundredPercentAvailabilityRestrictionAndShortBreakBug44956()
		{
			var firstDay = new DateOnly(2015, 10, 12); //mon
			var period = DateOnlyPeriod.CreateWithNumberOfWeeks(firstDay, 1);
			var phoneActivity = new Activity("phone") {InContractTime = true, InWorkTime = true};
			var shortBreakActivity = new Activity("break") { InContractTime = true, InWorkTime = false }; //common in US?
			var skill = new Skill().WithId().For(phoneActivity).IsOpen();
			var scenario = new Scenario("_");
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet =
				new WorkShiftRuleSet(new WorkShiftTemplateGenerator(phoneActivity, new TimePeriodWithSegment(8, 0, 8, 0, 15),
					new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			ruleSet.AddExtender(new ActivityAbsoluteStartExtender(shortBreakActivity, new TimePeriodWithSegment(0, 15, 0, 15, 15),
				new TimePeriodWithSegment(8, 0, 8, 0, 15)));
			var team = new Team { Site = new Site("_") };
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet, team, skill).WithSchedulePeriodOneWeek(firstDay);
			agent.SchedulePeriod(firstDay).SetDaysOff(2);
			var skillDays = skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay,
				5,
				1,
				5,
				5,
				5,
				25,
				5);

			var asses = Enumerable.Range(0, 7).Select(i => new PersonAssignment(agent, scenario, firstDay.AddDays(i)).ShiftCategory(shiftCategory).WithLayer(phoneActivity, new TimePeriod(8, 16))).ToArray();
			asses[4].SetDayOff(new DayOffTemplate()); //saturday, needed to make nightly rest solver to fail otherwise it will rescue us because it uses old classic scheduling that doesent have this bug
			asses[5].SetDayOff(new DayOffTemplate()); //saturday
			var assesAndRestrictions = new List<IScheduleData>(asses);
			var availabilityRestriction = new AvailabilityRestriction
			{
				WorkTimeLimitation = new WorkTimeLimitation(TimeSpan.FromHours(8), TimeSpan.FromHours(8)),
				NotAvailable = false
			};
			var restrctions = Enumerable.Range(0, 7).Select(i => new ScheduleDataRestriction(agent, availabilityRestriction, firstDay.AddDays(i))).ToArray();
			assesAndRestrictions.AddRange(restrctions);

			var stateHolder = SchedulerStateHolder.Fill(scenario, period, new []{agent}, assesAndRestrictions, skillDays);
			var optPrefs = new OptimizationPreferences { General = { ScheduleTag = new ScheduleTag(), UseAvailabilities = true, AvailabilitiesValue = 1} };

			Target.Execute(period, new[] { agent }, optPrefs, new FixedDayOffOptimizationPreferenceProvider(new DaysOffPreferences()), new NoOptimizationCallback());

			stateHolder.Schedules[agent].ScheduledDay(firstDay.AddDays(5)).HasDayOff()
				.Should().Be.False();//saturday
			stateHolder.Schedules[agent].ScheduledDay(firstDay.AddDays(1)).HasDayOff()
				.Should().Be.True();//tuesday
		}

		[TestCase(1d, true)]
		[TestCase(0.99d, true)]
		[TestCase(0d, false)]
		public void ShouldCareAboutHourlyAvailabilityRestrictions(double studentAvailabilityValue, bool agentShouldHaveDayOff)
		{
			var date = new DateOnly(2015, 10, 12);
			var period = DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1);
			var activity = new Activity();
			var skill = new Skill().WithId().For(activity).IsOpen();
			var scenario = new Scenario();
			var shiftCategory = new ShiftCategory().WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet, skill).WithSchedulePeriodOneWeek(date);
			var skillDays = skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, date, 5, 1, 5, 5, 5, 5, 25);
			var assesAndRestrictions = new List<IPersistableScheduleData>();
			for (var i = 0; i < 6; i++)
			{
				assesAndRestrictions.Add(new PersonAssignment(agent, scenario, date.AddDays(i)).ShiftCategory(shiftCategory).WithLayer(activity, new TimePeriod(8, 16)));
				assesAndRestrictions.Add(new StudentAvailabilityDay(agent, date.AddDays(i), new[]
				{
					new StudentAvailabilityRestriction
					{
						StartTimeLimitation = new StartTimeLimitation(new TimeSpan(4, 0, 0), null),
						EndTimeLimitation = new EndTimeLimitation(null, new TimeSpan(21, 0, 0))
					}
				}));
			}
			assesAndRestrictions.Add(new PersonAssignment(agent, scenario, date.AddDays(6)).WithDayOff());
			var stateHolder = SchedulerStateHolder.Fill(scenario, period, agent, assesAndRestrictions, skillDays);
			var optPrefs = new OptimizationPreferences { General = { ScheduleTag = new ScheduleTag(), UseStudentAvailabilities = true, StudentAvailabilitiesValue = studentAvailabilityValue}};

			Target.Execute(period, new[] {agent}, optPrefs,
				new FixedDayOffOptimizationPreferenceProvider(new DaysOffPreferences()),
				new NoOptimizationCallback());

			stateHolder.Schedules[agent].ScheduledDay(date.AddDays(6)).HasDayOff()
				.Should().Be.EqualTo(agentShouldHaveDayOff);
		}

		public DayOffOptimizationRestrictionDesktopTest(ResourcePlannerTestParameters resourcePlannerTestParameters) : base(resourcePlannerTestParameters)
		{
		}
	}
}