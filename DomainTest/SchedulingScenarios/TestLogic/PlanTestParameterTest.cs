using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.IocCommon.Toggle;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.TestLogic
{
	public class PlanTestParameterTest
	{
		[TestCase(SeperateWebRequest.SimulateSecondRequestOrScheduler, ExpectedResult = true)]
		[TestCase(SeperateWebRequest.SimulateFirstRequest, ExpectedResult = false)]
		[TestCase(null, ExpectedResult = false)]
		public bool ShouldSimulateSecondRequest(SeperateWebRequest? seperateWebRequest)
		{
			var target = new PlanTestParameters(new []{Toggles.TestToggle}, seperateWebRequest);

			return target.SimulateSecondRequest();
		}

		[Test]
		public void ShouldEnableToggles()
		{
			var activeToggles = new[] {Toggles.TestToggle, Toggles.TestToggle3};
			var toggleManager = new FakeToggleManager();
			var target = new PlanTestParameters(activeToggles, null);

			target.EnableToggles(toggleManager);
			
			toggleManager.IsEnabled(Toggles.TestToggle).Should().Be.True();
			toggleManager.IsEnabled(Toggles.TestToggle2).Should().Be.False();
			toggleManager.IsEnabled(Toggles.TestToggle3).Should().Be.True();
		}
	}
}