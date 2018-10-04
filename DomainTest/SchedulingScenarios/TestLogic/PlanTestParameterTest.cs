using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.TestLogic
{
	public class PlanTestParameterTest
	{
		[TestCase(SeperateWebRequest.SimulateSecondRequestOrScheduler, ExpectedResult = true)]
		[TestCase(SeperateWebRequest.SimulateFirstRequest, ExpectedResult = false)]
		[TestCase(null, ExpectedResult = false)]
		public bool ShouldSimulateSecondRequest(SeperateWebRequest? seperateWebRequest)
		{
			var iocTestContext = new FakeIoCTestContext();
			var target = new PlanTestParameters(new []{Toggles.TestToggle}, seperateWebRequest);

			target.SimulateNewRequest(iocTestContext);

			return iocTestContext.SimulateNewRequestWasCalled;
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

		private class FakeIoCTestContext : IIoCTestContext
		{
			public void SimulateShutdown()
			{
				throw new System.NotImplementedException();
			}
			public void SimulateRestart()
			{
				throw new System.NotImplementedException();
			}
			public bool SimulateNewRequestWasCalled { get; private set; }
			public void SimulateNewRequest()
			{
				SimulateNewRequestWasCalled = true;
			}
		}
	}
}