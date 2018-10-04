using NUnit.Framework;
using Teleopti.Ccc.Domain.FeatureFlags;

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

		[TestCase(ExpectedResult = false)]
		[TestCase(Toggles.TestToggle, ExpectedResult = true)]
		[TestCase(Toggles.TestToggle2, Toggles.TestToggle3, ExpectedResult = false)]
		public bool ShouldCheckIfToggleIsEnabled(params Toggles[] toggles)
		{
			var target = new PlanTestParameters(toggles, null);

			return target.IsEnabled(Toggles.TestToggle);
		}
	}
}