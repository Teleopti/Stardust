using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourcePlanner.Hints;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ResourcePlanner.Hints
{
	[DomainTest]
	[Toggle(Toggles.ResourcePlanner_SeamlessPlanningForPreferences_76288)]
	public class PreferenceHintTest
	{
		public CheckScheduleHints Target;
		public FakeScenarioRepository ScenarioRepository;
		
		[Test]
		public void ShouldSetSchedulingHintErrorData()
		{
			var period = new DateOnly(2000,1,1).ToDateOnlyPeriod();
			var agent = new Person().WithSchedulePeriodOneDay(period.StartDate).WithPersonPeriod()
				.WithName(new Name(Guid.NewGuid().ToString(), Guid.NewGuid().ToString())).WithId();
			var scenario = ScenarioRepository.Has();
			var currentSchedule = new ScheduleDictionaryForTest(scenario, period.ToDateTimePeriod(TimeZoneInfo.Utc));
			currentSchedule.AddScheduleData(agent, new PreferenceDay(agent, period.StartDate, new PreferenceRestriction
			{
				ShiftCategory = new ShiftCategory()
			}));
			currentSchedule.AddPersonAssignment(new PersonAssignment(agent, scenario, period.StartDate)
				.ShiftCategory(new ShiftCategory()).WithLayer(new Activity(), new TimePeriod(1, 2)));

			var schedulingHintError = Target.Execute(new HintInput(currentSchedule, new[] { agent }, period, null, false))
				.InvalidResources.Single();
			schedulingHintError.ResourceName.Should().Be.EqualTo(agent.Name.ToString());
			schedulingHintError.ValidationErrors.Single(x => x.ResourceType == ValidationResourceType.Preferences).ErrorResource
				.Should().Be.EqualTo(nameof(Resources.AgentScheduledWithoutPreferences));
		}
		
		[Test]
		public void ShouldNotRepeatTheSameHintWhenTwoSchedulePeriodsAreUsed()
		{
			var period = new DateOnlyPeriod(2000,1,1,2000,1,2);
			var agent = new Person().WithSchedulePeriodOneDay(period.StartDate).WithPersonPeriod()
				.WithName(new Name(Guid.NewGuid().ToString(), Guid.NewGuid().ToString())).WithId();
			var scenario = ScenarioRepository.Has();
			var currentSchedule = new ScheduleDictionaryForTest(scenario, period.ToDateTimePeriod(TimeZoneInfo.Utc));
			currentSchedule.AddScheduleData(agent, new PreferenceDay(agent, period.StartDate, new PreferenceRestriction
			{
				ShiftCategory = new ShiftCategory()
			}),new PreferenceDay(agent, period.EndDate, new PreferenceRestriction
			{
				ShiftCategory = new ShiftCategory()
			}));
			currentSchedule.AddPersonAssignment(new PersonAssignment(agent, scenario, period.StartDate)
				.ShiftCategory(new ShiftCategory()).WithLayer(new Activity(), new TimePeriod(1, 2)));
			currentSchedule.AddPersonAssignment(new PersonAssignment(agent, scenario, period.EndDate)
				.ShiftCategory(new ShiftCategory()).WithLayer(new Activity(), new TimePeriod(1, 2)));

			var schedulingHintError = Target.Execute(new HintInput(currentSchedule, new[] { agent }, period, null, false))
				.InvalidResources.Single();
			schedulingHintError.ResourceName.Should().Be.EqualTo(agent.Name.ToString());
			schedulingHintError.ValidationErrors.Single(x => x.ResourceType == ValidationResourceType.Preferences).ErrorResource
				.Should().Be.EqualTo(nameof(Resources.AgentScheduledWithoutPreferences));
		}
	}
}