using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Scheduling;

namespace Teleopti.Wfm.SchedulingTest.SchedulingScenarios.Scheduling
{
	[DomainTest]
	[UseIocForFatClient]
	public class MinMaxStaffingDesktopTest : SchedulingScenario
	{
		public DesktopScheduling Target;
		public Func<ISchedulerStateHolder> SchedulerStateHolderFrom;
		public FakeTimeZoneGuard TimeZoneGuard;
		public FakeUserTimeZone UserTimeZone;


		[Test]
		public void ShouldRespectMinimumStaffingWhenBreakingShiftCategoryLimiation()
		{
			var date = new DateOnly(2017, 1, 10);
			var activity = new Activity("_").WithId();
			var scenario = new Scenario("_");
			var skill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
			var skillDayOne = skill.CreateSkillDayWithDemand(scenario, date, 10);
			var skillDayTwo = skill.CreateSkillDayWithDemand(scenario, date.AddDays(1), 1); //lower demand but min staffing
			var shiftCat = new ShiftCategory("_");
			var shiftCategoryLimitation = new ShiftCategoryLimitation(shiftCat) { MaxNumberOf = 1 }; //makes it "not legal"
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), new ShiftCategory().WithId()));
			var agent = new Person().WithId()
								.InTimeZone(TimeZoneInfo.Utc)
								.WithPersonPeriod(ruleSet, skill)
								.WithSchedulePeriodOneWeek(date);
			agent.SchedulePeriod(date).AddShiftCategoryLimitation(shiftCategoryLimitation);
			var assDayOne = new PersonAssignment(agent, scenario, date).WithLayer(activity, new TimePeriod(8, 16)).ShiftCategory(shiftCat);
			var assDayTwo = new PersonAssignment(agent, scenario, date.AddDays(1)).WithLayer(activity, new TimePeriod(8, 16)).ShiftCategory(shiftCat);
			var schedulerStateHolder = SchedulerStateHolderFrom.Fill(scenario, new DateOnlyPeriod(date, date.AddDays(1)), new []{agent}, new[]{assDayOne, assDayTwo}, new[]{skillDayOne, skillDayTwo});
			foreach (var skillDataPeriod in skillDayTwo.SkillDataPeriodCollection)
			{
				skillDataPeriod.SkillPersonData = new SkillPersonData(1, 0); //at least one agent all daytwo
			}
			var schedulingOptions = new SchedulingOptions {UseMinimumStaffing = true};

			Target.Execute(new NoSchedulingCallback(),
				schedulingOptions,
				new NoSchedulingProgress(),
				new []{agent}, 
				new DateOnlyPeriod(date, date.AddDays(1))
			);

			schedulerStateHolder.Schedules[agent]
				.ScheduledDayCollection(DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1))
				.Count(x => shiftCat.Equals(x.PersonAssignment(true).ShiftCategory))
				.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldGetCorrectShiftWhenUsingMinimumStaffing()
		{
			var date = new DateOnly(2017, 1, 11);
			var activity = new Activity().WithId();
			var scenario = new Scenario();
			var shiftCategory = new ShiftCategory().WithId();
			var skill = new Skill().For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
			var skillDayOne = skill.CreateSkillDayWithDemand(scenario, date.AddDays(-1), 1);
			var skillDayTwo = skill.CreateSkillDayWithDemand(scenario, date, 1);
			var skillDayThree = skill.CreateSkillDayWithDemand(scenario, date.AddDays(1), 1);
			var shiftCat = new ShiftCategory("_");
			var ruleSet1 = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var ruleSet2 = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(18, 0, 18, 0, 15), new TimePeriodWithSegment(26, 0, 26, 0, 15), shiftCategory));
			var ruleSetBag = new RuleSetBag(ruleSet1, ruleSet2);
			var contract = new ContractWithMaximumTolerance();
			var agents = new List<IPerson>();
			var asses = new List<IPersonAssignment>();
			for (var i = 0; i < 5; i++)
			{
				agents.Add(new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSetBag, contract, skill).WithSchedulePeriodOneWeek(date));
			}
			asses.Add(new PersonAssignment(agents[0], scenario, date).WithLayer(activity, new TimePeriod(8, 16)).ShiftCategory(shiftCat));
			asses.Add(new PersonAssignment(agents[1], scenario, date).WithLayer(activity, new TimePeriod(18, 26)).ShiftCategory(shiftCat));
			asses.Add(new PersonAssignment(agents[2], scenario, date).WithLayer(activity, new TimePeriod(18, 26)).ShiftCategory(shiftCat));
			asses.Add(new PersonAssignment(agents[3], scenario, date).WithLayer(activity, new TimePeriod(18, 26)).ShiftCategory(shiftCat));
			var schedulerStateHolder = SchedulerStateHolderFrom.Fill(scenario, new DateOnlyPeriod(date.AddDays(-1), date.AddDays(1)), agents, asses, new[] { skillDayOne, skillDayTwo, skillDayThree });
			skillDayTwo.SkillDataPeriodCollection.ForEach(x => x.SkillPersonData = new SkillPersonData(1, 0));
			var schedulingOptions = new SchedulingOptions { UseMinimumStaffing = true };

			Target.Execute(new NoSchedulingCallback(), schedulingOptions, new NoSchedulingProgress(), new[] { agents[4] }, date.ToDateOnlyPeriod());

			schedulerStateHolder.Schedules[agents[4]].ScheduledDay(date).PersonAssignment().Period.StartDateTime.Hour.Should().Be.EqualTo(8);
		}

		[Test]
		public void ShouldGetCorrectShiftWhenUsingMinimumStaffingDifferentTimeZones(
			[Values("Taipei Standard Time", "UTC", "Mountain Standard Time")] string userTimeZone,
			[Values("Taipei Standard Time", "UTC", "Mountain Standard Time")] string userViewPointTimeZone,
			[Values("Taipei Standard Time", "UTC", "Mountain Standard Time")] string agentTimeZone,
			[Values("Taipei Standard Time", "UTC", "Mountain Standard Time")] string skillTimeZone)
		{
			TimeZoneGuard.SetTimeZone(TimeZoneInfo.FindSystemTimeZoneById(userTimeZone));
			UserTimeZone.Is(TimeZoneInfo.FindSystemTimeZoneById(userViewPointTimeZone));
			var date = new DateOnly(2017, 1, 11);
			var activity = new Activity().WithId();
			var scenario = new Scenario();
			var shiftCategory1 = new ShiftCategory().WithId();
			var shiftCategory2 = new ShiftCategory().WithId();
			var skill = new Skill().For(activity).InTimeZone(TimeZoneInfo.FindSystemTimeZoneById(skillTimeZone)).WithId().IsOpen();
			var skillDayOne = skill.CreateSkillDayWithDemand(scenario, date.AddDays(-1), 1);
			var skillDayTwo = skill.CreateSkillDayWithDemand(scenario, date, 1);
			var skillDayThree = skill.CreateSkillDayWithDemand(scenario, date.AddDays(1), 1);
			var ruleSet1 = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory1));
			var ruleSet2 = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(18, 0, 18, 0, 15), new TimePeriodWithSegment(26, 0, 26, 0, 15), shiftCategory2));
			var ruleSetBag = new RuleSetBag(ruleSet1, ruleSet2);
			var contract = new ContractWithMaximumTolerance();
			var agents = new List<IPerson>();
			var asses = new List<IPersonAssignment>();
			for (var i = 0; i < 5; i++)
			{
				agents.Add(new Person().WithId().InTimeZone(TimeZoneInfo.FindSystemTimeZoneById(agentTimeZone)).WithPersonPeriod(ruleSetBag, contract, skill).WithSchedulePeriodOneWeek(date));
			}
			asses.Add(new PersonAssignment(agents[0], scenario, date).WithLayer(activity, new TimePeriod(8, 16)).ShiftCategory(shiftCategory2));
			asses.Add(new PersonAssignment(agents[1], scenario, date).WithLayer(activity, new TimePeriod(18, 26)).ShiftCategory(shiftCategory2));
			asses.Add(new PersonAssignment(agents[2], scenario, date).WithLayer(activity, new TimePeriod(18, 26)).ShiftCategory(shiftCategory2));
			asses.Add(new PersonAssignment(agents[3], scenario, date).WithLayer(activity, new TimePeriod(18, 26)).ShiftCategory(shiftCategory2));
			var schedulerStateHolder = SchedulerStateHolderFrom.Fill(scenario, new DateOnlyPeriod(date.AddDays(-1), date.AddDays(1)), agents, asses, new[] { skillDayOne, skillDayTwo, skillDayThree });
			skillDayOne.SkillDataPeriodCollection.ForEach(x => x.SkillPersonData = new SkillPersonData(1, 0));
			skillDayTwo.SkillDataPeriodCollection.ForEach(x => x.SkillPersonData = new SkillPersonData(1, 0));
			var schedulingOptions = new SchedulingOptions { UseMinimumStaffing = true };

			Target.Execute(new NoSchedulingCallback(), schedulingOptions, new NoSchedulingProgress(), new[] { agents[4] }, date.ToDateOnlyPeriod());

			schedulerStateHolder.Schedules[agents[4]].ScheduledDay(date).PersonAssignment().ShiftCategory.Should().Be.EqualTo(shiftCategory1);
		}

		public MinMaxStaffingDesktopTest(ResourcePlannerTestParameters resourcePlannerTestParameters) : base(resourcePlannerTestParameters)
		{
		}
	}
}