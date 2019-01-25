using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Wfm.SchedulingTest.SchedulingScenarios
{
	public class ResourcePlannerTestParameterTest
	{
		[TestCase(SeparateWebRequest.SimulateSecondRequestOrScheduler, ExpectedResult = true)]
		[TestCase(SeparateWebRequest.SimulateFirstRequest, ExpectedResult = false)]
		[TestCase(null, ExpectedResult = false)]
		public bool ShouldSimulateSecondRequest(SeparateWebRequest? separateWebRequest)
		{
			var iocTestContext = new FakeIoCTestContext();
			var target = new ResourcePlannerTestParameters(new []{Toggles.TestToggle}, separateWebRequest);

			target.MightSimulateNewRequest(iocTestContext);

			return iocTestContext.SimulateNewRequestWasCalled;
		}

		[Test]
		public void ShouldEnableToggles()
		{
			var activeToggles = new[] {Toggles.TestToggle, Toggles.TestToggle3};
			var toggleManager = new FakeToggleManager();
			var target = new ResourcePlannerTestParameters(activeToggles, null);

			target.EnableToggles(toggleManager);
			
			toggleManager.IsEnabled(Toggles.TestToggle).Should().Be.True();
			toggleManager.IsEnabled(Toggles.TestToggle2).Should().Be.False();
			toggleManager.IsEnabled(Toggles.TestToggle3).Should().Be.True();
		}
		
		[TestCase(ExpectedResult = false)]
		[TestCase(Toggles.TestToggle, ExpectedResult = true)]
		[TestCase(Toggles.TestToggle2, Toggles.TestToggle3, ExpectedResult = false)]
		public bool ShouldCheckIfToggleIsEnabled(params Toggles[] toggles)
		{
			var target = new ResourcePlannerTestParameters(toggles, null);

			return target.IsEnabled(Toggles.TestToggle);
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