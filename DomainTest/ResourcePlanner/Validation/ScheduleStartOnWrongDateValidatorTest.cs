using System;
using NUnit.Framework;
using System.Linq;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourcePlanner.Validation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ResourcePlanner.Validation
{
	[DomainTest]
	[Toggle(Toggles.ResourcePlanner_ShowSwitchedTimeZone_46303)]
	public class ScheduleStartOnWrongDateValidatorTest : ISetup
	{
		public Func<ISchedulerStateHolder> StateHolder;
		public SchedulingValidator Target;

		[Test]
		public void ShouldJumpThroughIfScheduleIsNullToSupportCheapPreChecks()
		{
			Assert.DoesNotThrow(() =>
			{			
				Target.Validate(null, new[]{new Person(), }, DateOnly.Today.ToDateOnlyPeriod());
			});
		}

		[TestCase(1, "Mountain Standard Time", ExpectedResult = true)]
		[TestCase(8, "Mountain Standard Time", ExpectedResult = false)]
		[TestCase(23, "Mountain Standard Time", ExpectedResult = false)]
		[TestCase(1, "GMT Standard Time", ExpectedResult = false)]
		[TestCase(22, "GMT Standard Time", ExpectedResult = false)]
		[TestCase(1, "Singapore Standard Time", ExpectedResult = false)]
		[TestCase(8, "Singapore Standard Time", ExpectedResult = false)]
		[TestCase(16, "Singapore Standard Time", ExpectedResult = true)]
		[TestCase(23, "Singapore Standard Time", ExpectedResult = true)]
		public bool ShouldReturnValidationErrorIfAgentChangedToTimeZoneEarlier(int startHourOfPresentShift, string newTimezoneForAgent)
		{
			var scenario = new Scenario();
			var date = DateOnly.Today;
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc);
			var ass = new PersonAssignment(agent, scenario, date)
				.WithLayer(new Activity(), new TimePeriod(startHourOfPresentShift, startHourOfPresentShift + 8));
			var state = StateHolder.Fill(scenario, date, agent, ass);
			
			agent.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.FindSystemTimeZoneById(newTimezoneForAgent));
			
			return Target.Validate(state.Schedules, new[] {agent}, date.ToDateOnlyPeriod())
				.InvalidResources.Any(x => x.ValidationTypes.Contains(typeof(ScheduleStartOnWrongDateValidator)));
		}

		[TestCase("Mountain Standard Time")]
		[TestCase("GMT Standard Time")]
		[TestCase("Singapore Standard Time")]
		public void ShouldNotReturnValidationErrorIfScheduleDayMissPersonAssignment(string timezoneForAgent)
		{
			var scenario = new Scenario();
			var date = DateOnly.Today;
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.FindSystemTimeZoneById(timezoneForAgent));
			var state = StateHolder.Fill(scenario, date, agent);
			
			Target.Validate(state.Schedules, new[] {agent}, date.ToDateOnlyPeriod())
				.InvalidResources.Any(x => x.ValidationTypes.Contains(typeof(ScheduleStartOnWrongDateValidator)))
				.Should().Be.False();
		}

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble(new FakeScenarioRepository(ScenarioFactory.CreateScenario("Default", true, true))).For<IScenarioRepository>();
		}
	}
}