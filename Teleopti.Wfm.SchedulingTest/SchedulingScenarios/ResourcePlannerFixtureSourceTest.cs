using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.FeatureFlags;

namespace Teleopti.Wfm.SchedulingTest.SchedulingScenarios
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
				new ResourcePlannerTestParameters(Enumerable.Empty<Toggles>(), SeparateWebRequest.SimulateFirstRequest),
				new ResourcePlannerTestParameters(Enumerable.Empty<Toggles>(), SeparateWebRequest.SimulateSecondRequestOrScheduler)
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
				new ResourcePlannerTestParameters(Enumerable.Empty<Toggles>(), SeparateWebRequest.SimulateFirstRequest),
				new ResourcePlannerTestParameters(new[] {Toggles.TestToggle}, SeparateWebRequest.SimulateFirstRequest),
				new ResourcePlannerTestParameters(Enumerable.Empty<Toggles>(), SeparateWebRequest.SimulateSecondRequestOrScheduler),
				new ResourcePlannerTestParameters(new[] {Toggles.TestToggle}, SeparateWebRequest.SimulateSecondRequestOrScheduler)
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
				new ResourcePlannerTestParameters(Enumerable.Empty<Toggles>(), SeparateWebRequest.SimulateFirstRequest),
				new ResourcePlannerTestParameters(new[] {Toggles.TestToggle}, SeparateWebRequest.SimulateFirstRequest),
				new ResourcePlannerTestParameters(new[] {Toggles.TestToggle, Toggles.TestToggle2}, SeparateWebRequest.SimulateFirstRequest),
				new ResourcePlannerTestParameters(new[] {Toggles.TestToggle2}, SeparateWebRequest.SimulateFirstRequest),
				new ResourcePlannerTestParameters(Enumerable.Empty<Toggles>(), SeparateWebRequest.SimulateSecondRequestOrScheduler),
				new ResourcePlannerTestParameters(new[] {Toggles.TestToggle}, SeparateWebRequest.SimulateSecondRequestOrScheduler),
				new ResourcePlannerTestParameters(new[] {Toggles.TestToggle, Toggles.TestToggle2}, SeparateWebRequest.SimulateSecondRequestOrScheduler),
				new ResourcePlannerTestParameters(new[] {Toggles.TestToggle2}, SeparateWebRequest.SimulateSecondRequestOrScheduler)
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
				new ResourcePlannerTestParameters(Enumerable.Empty<Toggles>(), SeparateWebRequest.SimulateFirstRequest),
				new ResourcePlannerTestParameters(new[] {Toggles.TestToggle}, SeparateWebRequest.SimulateFirstRequest),
				new ResourcePlannerTestParameters(new[] {Toggles.TestToggle2}, SeparateWebRequest.SimulateFirstRequest),
				new ResourcePlannerTestParameters(new[] {Toggles.TestToggle3}, SeparateWebRequest.SimulateFirstRequest),
				new ResourcePlannerTestParameters(new[] {Toggles.TestToggle, Toggles.TestToggle2}, SeparateWebRequest.SimulateFirstRequest),
				new ResourcePlannerTestParameters(new[] {Toggles.TestToggle, Toggles.TestToggle3}, SeparateWebRequest.SimulateFirstRequest),
				new ResourcePlannerTestParameters(new[] {Toggles.TestToggle2, Toggles.TestToggle3}, SeparateWebRequest.SimulateFirstRequest),
				new ResourcePlannerTestParameters(new[] {Toggles.TestToggle, Toggles.TestToggle2, Toggles.TestToggle3}, SeparateWebRequest.SimulateFirstRequest),
				new ResourcePlannerTestParameters(Enumerable.Empty<Toggles>(), SeparateWebRequest.SimulateSecondRequestOrScheduler),
				new ResourcePlannerTestParameters(new[] {Toggles.TestToggle}, SeparateWebRequest.SimulateSecondRequestOrScheduler),
				new ResourcePlannerTestParameters(new[] {Toggles.TestToggle2}, SeparateWebRequest.SimulateSecondRequestOrScheduler),
				new ResourcePlannerTestParameters(new[] {Toggles.TestToggle3}, SeparateWebRequest.SimulateSecondRequestOrScheduler),
				new ResourcePlannerTestParameters(new[] {Toggles.TestToggle, Toggles.TestToggle2}, SeparateWebRequest.SimulateSecondRequestOrScheduler),
				new ResourcePlannerTestParameters(new[] {Toggles.TestToggle, Toggles.TestToggle3}, SeparateWebRequest.SimulateSecondRequestOrScheduler),
				new ResourcePlannerTestParameters(new[] {Toggles.TestToggle2, Toggles.TestToggle3}, SeparateWebRequest.SimulateSecondRequestOrScheduler),
				new ResourcePlannerTestParameters(new[] {Toggles.TestToggle, Toggles.TestToggle2, Toggles.TestToggle3}, SeparateWebRequest.SimulateSecondRequestOrScheduler)
			);
		}
		
		[Test]
		public void ThreeToggle_WithExtraRequest_VerifyCount()
		{
			var target = new resourcePlannerFixtureSourceForTest(new[]{Toggles.TestToggle, Toggles.TestToggle2, Toggles.TestToggle3}, true);

			target.Count().Should().Be.EqualTo(16);
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
		
		[TestCase(Toggles.TestToggle, Toggles.TestToggle2, Toggles.TestToggle3)]
		[TestCase(Toggles.TestToggle3, Toggles.TestToggle2, Toggles.TestToggle)]
		[TestCase(Toggles.TestToggle2, Toggles.TestToggle, Toggles.TestToggle3)]
		public void ThreeToggles_WithExtraRequest_CheckOrder_UpDown(Toggles toggle1, Toggles toggle2, Toggles toggle3)
		{
			var target = new resourcePlannerFixtureSourceForTest(new[]{toggle1, toggle2, toggle3}, true);

			target.Should().Have.SameSequenceAs(
				new ResourcePlannerTestParameters(Enumerable.Empty<Toggles>(), SeparateWebRequest.SimulateFirstRequest),
				new ResourcePlannerTestParameters(new[] {Toggles.TestToggle}, SeparateWebRequest.SimulateFirstRequest),
				new ResourcePlannerTestParameters(new[] {Toggles.TestToggle2}, SeparateWebRequest.SimulateFirstRequest),
				new ResourcePlannerTestParameters(new[] {Toggles.TestToggle3}, SeparateWebRequest.SimulateFirstRequest),
				new ResourcePlannerTestParameters(new[] {Toggles.TestToggle, Toggles.TestToggle2}, SeparateWebRequest.SimulateFirstRequest),
				new ResourcePlannerTestParameters(new[] {Toggles.TestToggle, Toggles.TestToggle3}, SeparateWebRequest.SimulateFirstRequest),
				new ResourcePlannerTestParameters(new[] {Toggles.TestToggle2, Toggles.TestToggle3}, SeparateWebRequest.SimulateFirstRequest),
				new ResourcePlannerTestParameters(new[] {Toggles.TestToggle, Toggles.TestToggle2, Toggles.TestToggle3}, SeparateWebRequest.SimulateFirstRequest),
				new ResourcePlannerTestParameters(Enumerable.Empty<Toggles>(), SeparateWebRequest.SimulateSecondRequestOrScheduler),
				new ResourcePlannerTestParameters(new[] {Toggles.TestToggle}, SeparateWebRequest.SimulateSecondRequestOrScheduler),
				new ResourcePlannerTestParameters(new[] {Toggles.TestToggle2}, SeparateWebRequest.SimulateSecondRequestOrScheduler),
				new ResourcePlannerTestParameters(new[] {Toggles.TestToggle3}, SeparateWebRequest.SimulateSecondRequestOrScheduler),
				new ResourcePlannerTestParameters(new[] {Toggles.TestToggle, Toggles.TestToggle2}, SeparateWebRequest.SimulateSecondRequestOrScheduler),
				new ResourcePlannerTestParameters(new[] {Toggles.TestToggle, Toggles.TestToggle3}, SeparateWebRequest.SimulateSecondRequestOrScheduler),
				new ResourcePlannerTestParameters(new[] {Toggles.TestToggle2, Toggles.TestToggle3}, SeparateWebRequest.SimulateSecondRequestOrScheduler),
				new ResourcePlannerTestParameters(new[] {Toggles.TestToggle, Toggles.TestToggle2, Toggles.TestToggle3}, SeparateWebRequest.SimulateSecondRequestOrScheduler)
			);
		}
		
		[Test]
		public void ThreeToggles_CheckOrder_LeftRight()
		{
			var target = new resourcePlannerFixtureSourceForTest(new[]{Toggles.TestToggle3, Toggles.TestToggle, Toggles.TestToggle2}, false);
			
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