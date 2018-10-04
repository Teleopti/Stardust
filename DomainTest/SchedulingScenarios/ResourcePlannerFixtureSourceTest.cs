using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.FeatureFlags;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios
{
	public class ResourcePlannerFixtureSourceTest
	{
		[Test]
		public void ZeroToggles()
		{
			var target = new resourcePlannerFixtureSourceForTest(Enumerable.Empty<Toggles>(), false);
			
			target.Should().Have.SameValuesAs(
				new ResourcePlannerTestParameters(Enumerable.Empty<Toggles>(), null)
			);
		}

		[Test]
		public void ZeroToggles_WithExtraRequest()
		{
			var target = new resourcePlannerFixtureSourceForTest(Enumerable.Empty<Toggles>(), true);
			
			target.Should().Have.SameValuesAs(
				new ResourcePlannerTestParameters(Enumerable.Empty<Toggles>(), SeperateWebRequest.SimulateFirstRequest),
				new ResourcePlannerTestParameters(Enumerable.Empty<Toggles>(), SeperateWebRequest.SimulateSecondRequestOrScheduler)
			);
		}
		
		[Test]
		public void OneToggle()
		{
			var target = new resourcePlannerFixtureSourceForTest(new[]{Toggles.TestToggle}, false);

			target.Should().Have.SameValuesAs(
				new ResourcePlannerTestParameters(Enumerable.Empty<Toggles>(), null),
				new ResourcePlannerTestParameters(new[] {Toggles.TestToggle}, null)
				);
		}
		
		[Test]
		public void OneToggle_WithExtraRequest()
		{
			var target = new resourcePlannerFixtureSourceForTest(new[]{Toggles.TestToggle}, true);

			target.Should().Have.SameValuesAs(
				new ResourcePlannerTestParameters(Enumerable.Empty<Toggles>(), SeperateWebRequest.SimulateFirstRequest),
				new ResourcePlannerTestParameters(new[] {Toggles.TestToggle}, SeperateWebRequest.SimulateFirstRequest),
				new ResourcePlannerTestParameters(Enumerable.Empty<Toggles>(), SeperateWebRequest.SimulateSecondRequestOrScheduler),
				new ResourcePlannerTestParameters(new[] {Toggles.TestToggle}, SeperateWebRequest.SimulateSecondRequestOrScheduler)
			);
		}

		[Test]
		public void TwoToggles()
		{
			var target = new resourcePlannerFixtureSourceForTest(new[]{Toggles.TestToggle, Toggles.TestToggle2}, false);
			
			target.Should().Have.SameValuesAs(
				new ResourcePlannerTestParameters(Enumerable.Empty<Toggles>(), null),
				new ResourcePlannerTestParameters(new[] {Toggles.TestToggle}, null),
				new ResourcePlannerTestParameters(new[] {Toggles.TestToggle, Toggles.TestToggle2}, null),
				new ResourcePlannerTestParameters(new[] {Toggles.TestToggle2}, null)
			);
		}
		
		[Test]
		public void TwoToggles_WithExtraRequest()
		{
			var target = new resourcePlannerFixtureSourceForTest(new[]{Toggles.TestToggle, Toggles.TestToggle2}, true);

			target.Should().Have.SameValuesAs(
				new ResourcePlannerTestParameters(Enumerable.Empty<Toggles>(), SeperateWebRequest.SimulateFirstRequest),
				new ResourcePlannerTestParameters(new[] {Toggles.TestToggle}, SeperateWebRequest.SimulateFirstRequest),
				new ResourcePlannerTestParameters(new[] {Toggles.TestToggle, Toggles.TestToggle2}, SeperateWebRequest.SimulateFirstRequest),
				new ResourcePlannerTestParameters(new[] {Toggles.TestToggle2}, SeperateWebRequest.SimulateFirstRequest),
				new ResourcePlannerTestParameters(Enumerable.Empty<Toggles>(), SeperateWebRequest.SimulateSecondRequestOrScheduler),
				new ResourcePlannerTestParameters(new[] {Toggles.TestToggle}, SeperateWebRequest.SimulateSecondRequestOrScheduler),
				new ResourcePlannerTestParameters(new[] {Toggles.TestToggle, Toggles.TestToggle2}, SeperateWebRequest.SimulateSecondRequestOrScheduler),
				new ResourcePlannerTestParameters(new[] {Toggles.TestToggle2}, SeperateWebRequest.SimulateSecondRequestOrScheduler)
			);
		}
		
		[Test]
		public void ThreeToggles()
		{
			var target = new resourcePlannerFixtureSourceForTest(new[]{Toggles.TestToggle, Toggles.TestToggle2, Toggles.TestToggle3}, false);
			
			target.Should().Have.SameValuesAs(
				new ResourcePlannerTestParameters(Enumerable.Empty<Toggles>(), null),
				new ResourcePlannerTestParameters(new[] {Toggles.TestToggle}, null),
				new ResourcePlannerTestParameters(new[] {Toggles.TestToggle, Toggles.TestToggle2}, null),
				new ResourcePlannerTestParameters(new[] {Toggles.TestToggle, Toggles.TestToggle3}, null),
				new ResourcePlannerTestParameters(new[] {Toggles.TestToggle, Toggles.TestToggle2, Toggles.TestToggle3}, null),
				new ResourcePlannerTestParameters(new[] {Toggles.TestToggle2}, null),
				new ResourcePlannerTestParameters(new[] {Toggles.TestToggle2, Toggles.TestToggle3}, null),
				new ResourcePlannerTestParameters(new[] {Toggles.TestToggle3}, null)
			);
		}
		
		[Test]
		public void ThreeToggles_WithExtraRequest()
		{
			var target = new resourcePlannerFixtureSourceForTest(new[]{Toggles.TestToggle, Toggles.TestToggle2, Toggles.TestToggle3}, true);

			target.Should().Have.SameValuesAs(
				new ResourcePlannerTestParameters(Enumerable.Empty<Toggles>(), SeperateWebRequest.SimulateFirstRequest),
				new ResourcePlannerTestParameters(new[] {Toggles.TestToggle}, SeperateWebRequest.SimulateFirstRequest),
				new ResourcePlannerTestParameters(new[] {Toggles.TestToggle2}, SeperateWebRequest.SimulateFirstRequest),
				new ResourcePlannerTestParameters(new[] {Toggles.TestToggle3}, SeperateWebRequest.SimulateFirstRequest),
				new ResourcePlannerTestParameters(new[] {Toggles.TestToggle, Toggles.TestToggle2}, SeperateWebRequest.SimulateFirstRequest),
				new ResourcePlannerTestParameters(new[] {Toggles.TestToggle, Toggles.TestToggle3}, SeperateWebRequest.SimulateFirstRequest),
				new ResourcePlannerTestParameters(new[] {Toggles.TestToggle2, Toggles.TestToggle3}, SeperateWebRequest.SimulateFirstRequest),
				new ResourcePlannerTestParameters(new[] {Toggles.TestToggle, Toggles.TestToggle2, Toggles.TestToggle3}, SeperateWebRequest.SimulateFirstRequest),
				new ResourcePlannerTestParameters(Enumerable.Empty<Toggles>(), SeperateWebRequest.SimulateSecondRequestOrScheduler),
				new ResourcePlannerTestParameters(new[] {Toggles.TestToggle}, SeperateWebRequest.SimulateSecondRequestOrScheduler),
				new ResourcePlannerTestParameters(new[] {Toggles.TestToggle2}, SeperateWebRequest.SimulateSecondRequestOrScheduler),
				new ResourcePlannerTestParameters(new[] {Toggles.TestToggle3}, SeperateWebRequest.SimulateSecondRequestOrScheduler),
				new ResourcePlannerTestParameters(new[] {Toggles.TestToggle, Toggles.TestToggle2}, SeperateWebRequest.SimulateSecondRequestOrScheduler),
				new ResourcePlannerTestParameters(new[] {Toggles.TestToggle, Toggles.TestToggle3}, SeperateWebRequest.SimulateSecondRequestOrScheduler),
				new ResourcePlannerTestParameters(new[] {Toggles.TestToggle2, Toggles.TestToggle3}, SeperateWebRequest.SimulateSecondRequestOrScheduler),
				new ResourcePlannerTestParameters(new[] {Toggles.TestToggle, Toggles.TestToggle2, Toggles.TestToggle3}, SeperateWebRequest.SimulateSecondRequestOrScheduler)
			);
		}

		[Test]
		public void FourToggles()
		{
			var target = new resourcePlannerFixtureSourceForTest(new[]{Toggles.TestToggle, Toggles.TestToggle2, Toggles.TestToggle3, Toggles.TestToggle4}, false);
			
			target.Should().Have.SameValuesAs(
				new ResourcePlannerTestParameters(Enumerable.Empty<Toggles>(), null),
				new ResourcePlannerTestParameters(new[] {Toggles.TestToggle}, null),
				new ResourcePlannerTestParameters(new[] {Toggles.TestToggle, Toggles.TestToggle2}, null),
				new ResourcePlannerTestParameters(new[] {Toggles.TestToggle, Toggles.TestToggle3}, null),
				new ResourcePlannerTestParameters(new[] {Toggles.TestToggle, Toggles.TestToggle4}, null),
				new ResourcePlannerTestParameters(new[] {Toggles.TestToggle, Toggles.TestToggle2, Toggles.TestToggle3}, null),
				new ResourcePlannerTestParameters(new[] {Toggles.TestToggle, Toggles.TestToggle2, Toggles.TestToggle4}, null),
				new ResourcePlannerTestParameters(new[] {Toggles.TestToggle, Toggles.TestToggle3, Toggles.TestToggle4}, null),
				new ResourcePlannerTestParameters(new[] {Toggles.TestToggle, Toggles.TestToggle2, Toggles.TestToggle3, Toggles.TestToggle4}, null),
				
				new ResourcePlannerTestParameters(new[] {Toggles.TestToggle2}, null),
				new ResourcePlannerTestParameters(new[] {Toggles.TestToggle2, Toggles.TestToggle3}, null),
				new ResourcePlannerTestParameters(new[] {Toggles.TestToggle2, Toggles.TestToggle4}, null),
				new ResourcePlannerTestParameters(new[] {Toggles.TestToggle2, Toggles.TestToggle3, Toggles.TestToggle4}, null),

				new ResourcePlannerTestParameters(new[] {Toggles.TestToggle3}, null),
				new ResourcePlannerTestParameters(new[] {Toggles.TestToggle3, Toggles.TestToggle4}, null),

				new ResourcePlannerTestParameters(new[] {Toggles.TestToggle4}, null)
			);
		}
		
		private class resourcePlannerFixtureSourceForTest : ResourcePlannerFixtureSource
		{
			public resourcePlannerFixtureSourceForTest(IEnumerable<Toggles> toggles, bool alsoSimulateSecondRequest)
			{
				ToggleFlags = toggles;
				AlsoSimulateSecondRequest = alsoSimulateSecondRequest;
			}
			
			protected override IEnumerable<Toggles> ToggleFlags { get; }
			protected override bool AlsoSimulateSecondRequest { get; }
		}
	}
}