using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.DayOffOptimization
{
	[TestFixture(false)]
	[TestFixture(true)]
	[DomainTest]
	public class DayOffOptimizationRestrictionTest : DayOffOptimizationScenario
	{
		private readonly bool _teamBlockDayOffForIndividuals;
		public Func<ISchedulerStateHolder> SchedulerStateHolder;
		public IDayOffOptimizationDesktop Target;

		public DayOffOptimizationRestrictionTest(bool teamBlockDayOffForIndividuals) : base(teamBlockDayOffForIndividuals)
		{
			_teamBlockDayOffForIndividuals = teamBlockDayOffForIndividuals;
		}

		[Test]
		public void ShouldMoveDaysOffWhenUsingHundredPercentAvailabilityRestrictionAndShortBreakBug44956()
		{
			var firstDay = new DateOnly(2015, 10, 12); //mon
			var period = DateOnlyPeriod.CreateWithNumberOfWeeks(firstDay, 1);
			var phoneActivity = new Activity("phone") {InContractTime = true, InWorkTime = true};
			var shortBreakActivity = new Activity("break") { InContractTime = true, InWorkTime = false }; //common in US?
			var skill = new Skill().For(phoneActivity).IsOpen();
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

			Target.Execute(period, new[] { agent }, new NoSchedulingProgress(), optPrefs, new FixedDayOffOptimizationPreferenceProvider(new DaysOffPreferences()), new GroupPageLight("_", GroupPageType.SingleAgent), () => new WorkShiftFinderResultHolder(), (o, args) => { });

			stateHolder.Schedules[agent].ScheduledDay(firstDay.AddDays(5)).HasDayOff()
				.Should().Be.False();//saturday
			stateHolder.Schedules[agent].ScheduledDay(firstDay.AddDays(1)).HasDayOff()
				.Should().Be.True();//tuesday
		}

		public override void Configure(FakeToggleManager toggleManager)
		{
			base.Configure(toggleManager);
			if (_teamBlockDayOffForIndividuals)
				toggleManager.Enable(Toggles.ResourcePlanner_TeamBlockDayOffForIndividuals_37998);
			else
				toggleManager.Disable(Toggles.ResourcePlanner_TeamBlockDayOffForIndividuals_37998);

		}
	}
}