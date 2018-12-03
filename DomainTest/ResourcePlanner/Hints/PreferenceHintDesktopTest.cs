using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
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


namespace Teleopti.Ccc.DomainTest.ResourcePlanner.Hints
{
	[DomainTest]
	[UseIocForFatClient]
	public class PreferenceHintDesktopTest
	{
		public CheckScheduleHints Target;
		public FakeScenarioRepository ScenarioRepository;

		[Test]
		public void ShouldNotCareAboutPreferenceHint()
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

			var result = Target.Execute(new SchedulePostHintInput(currentSchedule, new[] { agent }, period, null, false));
			result.InvalidResources.SelectMany(x => x.ValidationTypes)
				.Any(x => x == typeof(PreferenceHint))
				.Should().Be.False();
		}
	}
}