using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.DomainTest.SchedulingScenarios;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock
{
	[DomainTest]
	public class TeamBlockScheduleCommandTest
	{
		public Domain.Scheduling.Legacy.Commands.Scheduling Target;
		public Func<ISchedulerStateHolder> SchedulerStateHolder;
		public IResourceCalculation ResourceOptimizationHelper;
		public Func<IScheduleDayChangeCallback> ScheduleDayChangeCallback;
		public IGroupPersonBuilderForOptimizationFactory GroupPersonBuilderForOptimizationFactory;
		public FakeBusinessUnitRepository BusinessUnitRepository;

		[Test]
		public void ShouldHandleBlockSamerStartTimeInCombinationWithRotationWithSpecifyedShiftCategoryOnBlockStartingDateBug41378()
		{
			var firstDay = new DateOnly(2015, 10, 12);
			var period = DateOnlyPeriod.CreateWithNumberOfWeeks(firstDay, 1);
			var scenario = new Scenario("_");
			var phoneActivity = new Activity("_");
			var skill = SkillFactory.CreateSkillWithId("skill");
			skill.TimeZone = TimeZoneInfo.Utc;
			skill.Activity = phoneActivity;
			WorkloadFactory.CreateWorkloadWithOpenHours(skill, new TimePeriod(12, 0, 21, 0));

			var skilldays = skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay,
				10, 10, 10, 10, 10, 10, 10);

			BusinessUnitRepository.Has(ServiceLocatorForEntity.CurrentBusinessUnit.Current());
			var team1 = new Team().WithDescription(new Description("team1"));

			var shiftCategory8H15M = new ShiftCategory("L").WithId();
			var ruleSet8H15M =
				new WorkShiftRuleSet(new WorkShiftTemplateGenerator(phoneActivity, new TimePeriodWithSegment(12, 0, 12, 0, 15),
					new TimePeriodWithSegment(20, 15, 20, 15, 15), shiftCategory8H15M));
			var ruleSetBag = new RuleSetBag(ruleSet8H15M);
			var shiftCategory7H = new ShiftCategory("S").WithId();
			var ruleSet7H =
				new WorkShiftRuleSet(new WorkShiftTemplateGenerator(phoneActivity, new TimePeriodWithSegment(12, 0, 12, 0, 15),
					new TimePeriodWithSegment(19, 0, 19, 0, 15), shiftCategory7H));
			ruleSetBag.AddRuleSet(ruleSet7H);

			var agent1 = PersonFactory.CreatePersonWithPersonPeriod(firstDay, new[] { skill }).WithId();
			agent1.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);

			var schedulePeriod = new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1);
			schedulePeriod.SetDaysOff(2);

			agent1.AddSchedulePeriod(schedulePeriod);
			var personPeriod = agent1.Period(firstDay);
			personPeriod.Team = team1;
			personPeriod.RuleSetBag = ruleSetBag;
			personPeriod.PersonContract.Contract.EmploymentType = EmploymentType.FixedStaffDayWorkTime;
			//Add rotation

			var dayOffTemplate = new DayOffTemplate(new Description("DO", "DO"));
			var personAssignmentMonday = new PersonAssignment(agent1, scenario, firstDay);
			personAssignmentMonday.SetDayOff(dayOffTemplate);
			var personAssignmentSunday = new PersonAssignment(agent1, scenario, firstDay.AddDays(6));
			personAssignmentSunday.SetDayOff(dayOffTemplate);

			var stateHolder = SchedulerStateHolder.Fill(scenario, period, new[] { agent1 },
				new[] { personAssignmentMonday, personAssignmentSunday }, skilldays);

			var rotation = new Rotation("_", 7);
			rotation.RotationDays[1].RestrictionCollection[0].ShiftCategory = shiftCategory7H;
			var personRotation = new ScheduleDataRestriction(agent1, rotation.RotationDays[1].RestrictionCollection[0], firstDay.AddDays(1));
			((ScheduleRange)stateHolder.Schedules[personRotation.Person]).Add(personRotation);
			stateHolder.ResetFilteredPersons();

			var schedulingOptions = new SchedulingOptions
			{
				UseAvailability = false,
				UsePreferences = false,
				UseRotations = true, //Set to true
				UseStudentAvailability = false,
				DayOffTemplate = dayOffTemplate,
				ScheduleEmploymentType = ScheduleEmploymentType.FixedStaff,
				GroupOnGroupPageForTeamBlockPer = new GroupPageLight("SingleAgent", GroupPageType.SingleAgent),
				UseTeam = false,
				TeamSameShiftCategory = false,
				TagToUseOnScheduling = NullScheduleTag.Instance,
				UseBlock = true,
				BlockFinderTypeForAdvanceScheduling = BlockFinderType.BetweenDayOff,
				BlockSameStartTime = true,
				BlockSameShiftCategory = false,
				UseAverageShiftLengths = true
			};

			var blockPreferenceProvider = new FixedBlockPreferenceProvider(new ExtraPreferences()
			{
				UseBlockSameStartTime = schedulingOptions.BlockSameStartTime,
				UseBlockSameShift = schedulingOptions.BlockSameShift,
				UseBlockSameShiftCategory = schedulingOptions.BlockSameShiftCategory,
				BlockTypeValue = schedulingOptions.BlockFinderTypeForAdvanceScheduling
			});

			Target.Execute(new NoSchedulingCallback(), schedulingOptions, new NoSchedulingProgress(), new List<IPerson> { agent1 }, period, blockPreferenceProvider);

			var schedules = stateHolder.Schedules[agent1].ScheduledDayCollection(period).ToList();
			schedules[0].PersonAssignment().AssignedWithDayOff(dayOffTemplate).Should().Be.True();
			schedules[1].PersonAssignment().ShiftCategory.Should().Be.EqualTo(shiftCategory7H);
			schedules[2].PersonAssignment().ShiftCategory.Should().Be.EqualTo(shiftCategory8H15M);
			schedules[3].PersonAssignment().ShiftCategory.Should().Be.EqualTo(shiftCategory8H15M);
			schedules[4].PersonAssignment().ShiftCategory.Should().Be.EqualTo(shiftCategory8H15M);
			schedules[5].PersonAssignment().ShiftCategory.Should().Be.EqualTo(shiftCategory8H15M);
			schedules[6].PersonAssignment().AssignedWithDayOff(dayOffTemplate).Should().Be.True();
		}
	}
}