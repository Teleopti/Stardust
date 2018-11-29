using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.ResourcePlanner.Hints;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.UserTexts;


namespace Teleopti.Ccc.DomainTest.ResourcePlanner.Hints
{
	[DomainTest]
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

			var schedulingHintError = Target.Execute(new SchedulePostHintInput(currentSchedule, new[] { agent }, period, null, false))
				.InvalidResources.Single();
			schedulingHintError.ResourceName.Should().Be.EqualTo(agent.Name.ToString());
			schedulingHintError.ValidationErrors.Single(x => x.ResourceType == ValidationResourceType.Preferences).ErrorResource
				.Should().Be.EqualTo(nameof(Resources.AgentScheduledWithoutPreferences));
		}
		
		[Test]
		public void ShouldNotDisplayHintWhenNotAllDaysGetScheduled()
		{
			var period = DateOnlyPeriod.CreateWithNumberOfWeeks(new DateOnly(2018,10,15), 1);
			var agent = new Person().WithSchedulePeriodOneWeek(period.StartDate).WithPersonPeriod(new ContractScheduleWorkingMondayToFriday(),new Skill())
				.WithName(new Name(Guid.NewGuid().ToString(), Guid.NewGuid().ToString())).WithId();
			var scenario = ScenarioRepository.Has();
			var currentSchedule = new ScheduleDictionaryForTest(scenario, period.ToDateTimePeriod(TimeZoneInfo.Utc));
			currentSchedule.AddScheduleData(agent, new PreferenceDay(agent, period.StartDate, new PreferenceRestriction
			{
				DayOffTemplate = new DayOffTemplate()
			}));
			currentSchedule.AddPersonAssignment(new PersonAssignment(agent, scenario, period.StartDate.AddDays(5)).WithDayOff());
			currentSchedule.AddPersonAssignment(new PersonAssignment(agent, scenario, period.StartDate.AddDays(6)).WithDayOff());

			var schedulingHintError = Target.Execute(new SchedulePostHintInput(currentSchedule, new[] { agent }, period, null, false))
				.InvalidResources.Single();
			schedulingHintError.ResourceName.Should().Be.EqualTo(agent.Name.ToString());
			schedulingHintError.ValidationErrors.SingleOrDefault(x => x.ResourceType == ValidationResourceType.Preferences).Should().Be.Null();
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

			var schedulingHintError = Target.Execute(new SchedulePostHintInput(currentSchedule, new[] { agent }, period, null, false))
				.InvalidResources.Single();
			schedulingHintError.ResourceName.Should().Be.EqualTo(agent.Name.ToString());
			schedulingHintError.ValidationErrors.Single(x => x.ResourceType == ValidationResourceType.Preferences).ErrorResource
				.Should().Be.EqualTo(nameof(Resources.AgentScheduledWithoutPreferences));
		}
	}
}