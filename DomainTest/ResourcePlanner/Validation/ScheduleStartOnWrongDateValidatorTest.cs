using System;
using NUnit.Framework;
using System.Linq;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
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
		public Func<ISchedulerStateHolder> StateHolder; //just a way to be able to create a Ischeduledictionary... MAybe some easier way? Claes?
		public SchedulingValidator Target;

		[Test]
		public void ShouldJumpThroughIfScheduleIsNullToSupportCheapPreChecks()
		{
			Assert.DoesNotThrow(() =>
			{			
				Target.Validate(null, new[]{new Person(), }, DateOnly.Today.ToDateOnlyPeriod());
			});
		}

		[Test]
		[Ignore("to_be_continued")]
		public void ShouldReturnValidationErrorWhenAgentChangedToTimeZoneEarlier()
		{
			var scenario = new Scenario();
			var date = DateOnly.Today;
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc);
			var ass = new PersonAssignment(agent, scenario, date).ShiftCategory(new ShiftCategory("_")).WithLayer(new Activity(), new TimePeriod(8, 17));
			var state = StateHolder.Fill(scenario, date, agent, ass, null);
			
			agent.PermissionInformation.SetDefaultTimeZone(TimeZoneInfoFactory.HawaiiTimeZoneInfo());
			var result = Target.Validate(state.Schedules, new[] {agent}, date.ToDateOnlyPeriod());
			
			result.InvalidResources.Any(x => x.ValidationTypes.Contains(typeof(ScheduleStartOnWrongDateValidator)))
				.Should().Be.True();

		}

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble(new FakeScenarioRepository(ScenarioFactory.CreateScenario("Default", true, true))).For<IScenarioRepository>();
		}
		
	}
}