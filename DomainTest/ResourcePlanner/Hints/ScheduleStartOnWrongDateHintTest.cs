using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourcePlanner.Hints;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Scheduling;
using Teleopti.Ccc.UserTexts;


namespace Teleopti.Ccc.DomainTest.ResourcePlanner.Hints
{
	[DomainTest]
	public class ScheduleStartOnWrongDateHintTest : IIsolateSystem
	{
		public Func<ISchedulerStateHolder> StateHolder;
		public CheckScheduleHints Target;

		[Test]
		public void ShouldJumpThroughIfScheduleIsNullToSupportCheapPreChecks()
		{
			Assert.DoesNotThrow(() =>
			{			
				Target.Execute(new ScheduleHintInput(new[]{new Person(), }, DateOnly.Today.ToDateOnlyPeriod(), false));
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
		public bool ShouldReturnValidationErrorIfAgentChangedToTimeZone(int startHourOfPresentShift, string newTimezoneForAgent)
		{
			var scenario = new Scenario();
			var date = DateOnly.Today;
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc);
			var ass = new PersonAssignment(agent, scenario, date)
				.WithLayer(new Activity(), new TimePeriod(startHourOfPresentShift, startHourOfPresentShift + 8));
			var state = StateHolder.Fill(scenario, date, agent, ass);
			
			agent.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.FindSystemTimeZoneById(newTimezoneForAgent));
			
			return Target.Execute(new SchedulePostHintInput(state.Schedules, new[] {agent}, date.ToDateOnlyPeriod(), null, false))
				.InvalidResources.Any(x => x.ValidationTypes.Contains(typeof(ScheduleStartOnWrongDateHint)));
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
			
			Target.Execute(new SchedulePostHintInput(state.Schedules, new[] {agent}, date.ToDateOnlyPeriod(), null, false))
				.InvalidResources.Any(x => x.ValidationTypes.Contains(typeof(ScheduleStartOnWrongDateHint)))
				.Should().Be.False();
		}

		[Test]
		public void VerifyValidationErrorProperties()
		{
			var scenario = new Scenario();
			var date = DateOnly.Today;
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc);
			var ass = new PersonAssignment(agent, scenario, date)
				.WithLayer(new Activity(), new TimePeriod(1, 9));
			var state = StateHolder.Fill(scenario, date, agent, ass);
			
			agent.PermissionInformation.SetDefaultTimeZone(TimeZoneInfoFactory.DenverTimeZoneInfo());
			var result = Target.Execute(new SchedulePostHintInput(state.Schedules, new[] {agent}, date.ToDateOnlyPeriod(), null, false))
				.InvalidResources.Single();

			result.ResourceId.Should().Be.EqualTo(agent.Id.Value);
			result.ResourceName.Should().Be.EqualTo(agent.Name.ToString(NameOrderOption.FirstNameLastName));
			result.ValidationErrors.First().ResourceType.Should().Be.EqualTo(ValidationResourceType.Basic);
			result.ValidationErrors.Any(x => string.Format(Resources.ShiftStartsDayBeforeOrAfter, date.Date).Equals(HintsHelper.BuildErrorMessage(x, new SpecificTimeZone(TimeZoneInfoFactory.UtcTimeZoneInfo())))).Should().Be.True();
		}

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble(new FakeScenarioRepository(ScenarioFactory.CreateScenario("Default", true, true))).For<IScenarioRepository>();
		}
	}
}