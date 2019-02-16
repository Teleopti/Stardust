using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Scheduling;

namespace Teleopti.Wfm.SchedulingTest.SchedulingScenarios.Scheduling
{
	[DomainTest]
	[UseIocForFatClient]
	public class ResourceCalculationsInExtremeTimeZonesTest : SchedulingScenario
	{
		public DesktopScheduling Target;
		public Func<ISchedulerStateHolder> SchedulerStateHolderFrom;
		public FakeTimeZoneGuard TimeZoneGuard;

		[Test, Ignore("bug #81679")]
		public void
			ShouldRemoveShiftToFulfillLimitationAndResourceCalculateCorrectIfExtremeDifferenceInAgentAndAgdasTimezone()
		{
			var date = new DateOnly(2018, 6, 11);
			var shiftCategory = new ShiftCategory("Limited").WithId();
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var contract = new Contract("_")
			{
				WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(10), TimeSpan.FromHours(83),
					TimeSpan.FromHours(11), TimeSpan.FromHours(16))
			};
			var skill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).IsOpen().WithId();
			var skillDays = skill.CreateSkillDayWithDemand(scenario, new DateOnlyPeriod(2018, 6, 11, 2018, 6, 17), 1);

			var startTimePeriod = new TimePeriod(23, 23);
			var endTimePeriod = new TimePeriod(31, 31);
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity,
				new TimePeriodWithSegment(startTimePeriod, TimeSpan.FromMinutes(15)),
				new TimePeriodWithSegment(endTimePeriod, TimeSpan.FromMinutes(15)), shiftCategory));
			var agent = new Person().WithSchedulePeriodOneWeek(date).WithPersonPeriod(ruleSet, contract, skill)
				.InTimeZone(TimeZoneInfo.FindSystemTimeZoneById("Aleutian Standard Time")).WithId();
			agent.SchedulePeriod(date).DaysOff = 2;

			var assA = new PersonAssignment(agent, scenario, date.AddDays(5)).WithDayOff();
			var assB = new PersonAssignment(agent, scenario, date.AddDays(6)).WithDayOff();

			SchedulerStateHolderFrom.Fill(scenario, date.ToDateOnlyPeriod(), new[] {agent}, new[] {assA, assB}, skillDays);

			var schedulingOptionsTeamSingleAgent = new SchedulingOptions
			{
				GroupOnGroupPageForTeamBlockPer = new GroupPageLight("not interesting", GroupPageType.SingleAgent),
				UseTeam = true,
				UseBlock = false,
				UseShiftCategoryLimitations = true
			};
			TimeZoneGuard.Set(TimeZoneInfo.FindSystemTimeZoneById("Aleutian Standard Time"));
			Target.Execute(new NoSchedulingCallback(), schedulingOptionsTeamSingleAgent,
				new NoSchedulingProgress(), new[] {agent}, date.ToDateOnlyPeriod());

			TimeSpan? timeForDay;
			timeForDay = scheduledTimeOnSkillDay(skillDays[0]);
			timeForDay.Value.Should().Be.EqualTo(TimeSpan.Zero);

			timeForDay = scheduledTimeOnSkillDay(skillDays[1]);
			timeForDay.Value.Should().Be.EqualTo(TimeSpan.FromHours(8));

			timeForDay = scheduledTimeOnSkillDay(skillDays[2]);
			timeForDay.Value.Should().Be.EqualTo(TimeSpan.Zero);

			agent.SchedulePeriod(date)
				.AddShiftCategoryLimitation(new ShiftCategoryLimitation(shiftCategory) {MaxNumberOf = 0});

			TimeZoneGuard.Set(TimeZoneInfo.FindSystemTimeZoneById("Line Islands Standard Time"));
			Target.Execute(new NoSchedulingCallback(), schedulingOptionsTeamSingleAgent,
				new NoSchedulingProgress(), new[] {agent}, date.ToDateOnlyPeriod());

			timeForDay = scheduledTimeOnSkillDay(skillDays[0]);
			timeForDay.Value.Should().Be.EqualTo(TimeSpan.Zero);

			timeForDay = scheduledTimeOnSkillDay(skillDays[1]);
			timeForDay.Value.Should().Be.EqualTo(TimeSpan.Zero);

			timeForDay = scheduledTimeOnSkillDay(skillDays[2]);
			timeForDay.Value.Should().Be.EqualTo(TimeSpan.Zero);
		}

		private TimeSpan? scheduledTimeOnSkillDay(ISkillDay skillDay)
		{
			return skillDay.SkillStaffPeriodCollection
				.Select(skillStaffPeriod =>
					TimeSpan.FromMinutes(skillStaffPeriod.CalculatedResource *
										 skillStaffPeriod.Period.ElapsedTime().TotalMinutes))
				.Aggregate<TimeSpan, TimeSpan?>(null,
					(current, toAdd) => !current.HasValue ? toAdd : current.Value.Add(toAdd));
		}

		public ResourceCalculationsInExtremeTimeZonesTest(ResourcePlannerTestParameters resourcePlannerTestParameters) : base(resourcePlannerTestParameters)
		{
		}
	}
}