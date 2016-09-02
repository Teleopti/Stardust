using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.DomainTest.SchedulingScenarios;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock
{
	[DomainTest]
	public class TeamBlockScheduleCommandTest : ISetup
	{
		public ITeamBlockScheduleCommand Target;
		public Func<ISchedulerStateHolder> SchedulerStateHolder;
		public IResourceOptimizationHelper ResourceOptimizationHelper;
		public Func<IScheduleDayChangeCallback> ScheduleDayChangeCallback;
		public IGroupPersonBuilderForOptimizationFactory GroupPersonBuilderForOptimizationFactory;

		[Test]
		public void ShouldBeAbleToScheduleTeamWithAllMembersLoadedButOneMemberFilteredOut()
		{
			//two agents in the same team, everything equal
			//both agents in PersonsInOrganization
			//one agent in filteredPersons
			var firstDay = new DateOnly(2015, 10, 12);
			var period = new DateOnlyPeriod(firstDay, firstDay.AddWeeks(1));
			var scenario = new Scenario("_");
			var phoneActivity = new Activity("_");
			var skill = SkillFactory.CreateSkillWithId("skill");
			skill.TimeZone = TimeZoneInfo.Utc;
			skill.Activity = phoneActivity;
			WorkloadFactory.CreateWorkloadWithOpenHours(skill, new TimePeriod(12, 0, 20, 0));

			var skilldays = skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay,
				10,
				10,
				10,
				10,
				10,
				10,
				10);

			var businessUnit = BusinessUnitFactory.BusinessUnitUsedInTest;
			var site = new Site("site");
			var team1 = new Team {Description = new Description("team1")};
			site.AddTeam(team1);
			businessUnit.AddSite(site);

			var shiftCategory = new ShiftCategory("_").WithId();
			var normalRuleSet =
				new WorkShiftRuleSet(new WorkShiftTemplateGenerator(phoneActivity, new TimePeriodWithSegment(12, 0, 12, 0, 15),
					new TimePeriodWithSegment(20, 0, 20, 0, 15), shiftCategory));
			var ruleSetBag = new RuleSetBag(normalRuleSet);

			var agent1 = PersonFactory.CreatePersonWithPersonPeriod(firstDay, new[] {skill}).WithId();
			agent1.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			agent1.AddSchedulePeriod(new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1));
			agent1.Period(firstDay).Team = team1;
			agent1.Period(firstDay).RuleSetBag = ruleSetBag;

			var agent2 = PersonFactory.CreatePersonWithPersonPeriod(firstDay, new[] {skill}).WithId();
			agent2.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			agent2.AddSchedulePeriod(new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1));
			agent2.Period(firstDay).Team = team1;
			agent2.Period(firstDay).RuleSetBag = ruleSetBag;

			var stateHolder = SchedulerStateHolder.Fill(scenario, period, new[] {agent1, agent2},
				Enumerable.Empty<IPersonAssignment>(), skilldays);
			stateHolder.ResetFilteredPersons();
			stateHolder.FilterPersons(new List<IPerson> {agent1});

			var schedulingOptions = new SchedulingOptions
			{
				UseAvailability = true,
				UsePreferences = true,
				UseRotations = true,
				UseStudentAvailability = false,
				DayOffTemplate = new DayOffTemplate(),
				ScheduleEmploymentType = ScheduleEmploymentType.FixedStaff,
				GroupOnGroupPageForTeamBlockPer = new GroupPageLight(UserTexts.Resources.Main, GroupPageType.Hierarchy),
				UseTeam = true,
				TeamSameShiftCategory = true,
				TagToUseOnScheduling = NullScheduleTag.Instance
			};

			var selectedSchedules = stateHolder.Schedules[agent1].ScheduledDayCollection(period).ToList();
			var resourceCalculateDelayer = new ResourceCalculateDelayer(ResourceOptimizationHelper, 1, schedulingOptions.ConsiderShortBreaks, null); //CHECK THIS

			ISchedulePartModifyAndRollbackService rollbackService =
				new SchedulePartModifyAndRollbackService(stateHolder.SchedulingResultState,
					ScheduleDayChangeCallback(),
					new ScheduleTagSetter(schedulingOptions.TagToUseOnScheduling));
			var dayOffOptimizePreferenceProvider = new FixedDayOffOptimizationPreferenceProvider(new DaysOffPreferences());
			var result = Target.Execute(schedulingOptions, new NoSchedulingProgress(), new List<IPerson> {agent1},
				selectedSchedules, rollbackService, resourceCalculateDelayer, dayOffOptimizePreferenceProvider);
			result.GetResults(true, true).Count.Should().Be.EqualTo(0);
		}

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			//TODO: Remove this!
			system.UseTestDouble<FakeGroupScheduleGroupPageDataProvider>().For<IGroupScheduleGroupPageDataProvider>();
		}
	}
}