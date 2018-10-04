using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.FeatureFlags;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.TestLogic
{
	public class PlanFixtureSourceTest
	{
		[Test]
		public void ZeroToggles()
		{
			var target = new PlanFixtureSourceForTest(Enumerable.Empty<Toggles>(), false);
			
			target.Should().Have.SameValuesAs(
				new PlanTestParameters(Enumerable.Empty<Toggles>(), null)
			);
		}

		[Test]
		public void ZeroToggles_WithExtraRequest()
		{
			var target = new PlanFixtureSourceForTest(Enumerable.Empty<Toggles>(), true);
			
			target.Should().Have.SameValuesAs(
				new PlanTestParameters(Enumerable.Empty<Toggles>(), SeperateWebRequest.SimulateFirstRequest),
				new PlanTestParameters(Enumerable.Empty<Toggles>(), SeperateWebRequest.SimulateSecondRequestOrScheduler)
			);
		}
		
		[Test]
		public void OneToggle()
		{
			var target = new PlanFixtureSourceForTest(new[]{Toggles.TestToggle}, false);

			target.Should().Have.SameValuesAs(
				new PlanTestParameters(Enumerable.Empty<Toggles>(), null),
				new PlanTestParameters(new[] {Toggles.TestToggle}, null)
				);
		}
		
		[Test]
		public void OneToggle_WithExtraRequest()
		{
			var target = new PlanFixtureSourceForTest(new[]{Toggles.TestToggle}, true);

			target.Should().Have.SameValuesAs(
				new PlanTestParameters(Enumerable.Empty<Toggles>(), SeperateWebRequest.SimulateFirstRequest),
				new PlanTestParameters(new[] {Toggles.TestToggle}, SeperateWebRequest.SimulateFirstRequest),
				new PlanTestParameters(Enumerable.Empty<Toggles>(), SeperateWebRequest.SimulateSecondRequestOrScheduler),
				new PlanTestParameters(new[] {Toggles.TestToggle}, SeperateWebRequest.SimulateSecondRequestOrScheduler)
			);
		}

		[Test]
		public void TwoToggles()
		{
			var target = new PlanFixtureSourceForTest(new[]{Toggles.TestToggle, Toggles.TestToggle2}, false);

			var res = target.ToList();
			
			target.Should().Have.SameValuesAs(
				new PlanTestParameters(Enumerable.Empty<Toggles>(), null),
				new PlanTestParameters(new[] {Toggles.TestToggle}, null),
				new PlanTestParameters(new[] {Toggles.TestToggle, Toggles.TestToggle2}, null),
				new PlanTestParameters(new[] {Toggles.TestToggle2}, null)
			);
		}
		
		[Test]
		public void TwoToggles_WithExtraRequest()
		{
			var target = new PlanFixtureSourceForTest(new[]{Toggles.TestToggle, Toggles.TestToggle2}, true);

			target.Should().Have.SameValuesAs(
				new PlanTestParameters(Enumerable.Empty<Toggles>(), SeperateWebRequest.SimulateFirstRequest),
				new PlanTestParameters(new[] {Toggles.TestToggle}, SeperateWebRequest.SimulateFirstRequest),
				new PlanTestParameters(new[] {Toggles.TestToggle, Toggles.TestToggle2}, SeperateWebRequest.SimulateFirstRequest),
				new PlanTestParameters(new[] {Toggles.TestToggle2}, SeperateWebRequest.SimulateFirstRequest),
				new PlanTestParameters(Enumerable.Empty<Toggles>(), SeperateWebRequest.SimulateSecondRequestOrScheduler),
				new PlanTestParameters(new[] {Toggles.TestToggle}, SeperateWebRequest.SimulateSecondRequestOrScheduler),
				new PlanTestParameters(new[] {Toggles.TestToggle, Toggles.TestToggle2}, SeperateWebRequest.SimulateSecondRequestOrScheduler),
				new PlanTestParameters(new[] {Toggles.TestToggle2}, SeperateWebRequest.SimulateSecondRequestOrScheduler)
			);
		}
		
		[Test]
		public void ThreeToggles()
		{
			var target = new PlanFixtureSourceForTest(new[]{Toggles.TestToggle, Toggles.TestToggle2, Toggles.TestToggle3}, false);
			
			target.Should().Have.SameValuesAs(
				new PlanTestParameters(Enumerable.Empty<Toggles>(), null),
				new PlanTestParameters(new[] {Toggles.TestToggle}, null),
				new PlanTestParameters(new[] {Toggles.TestToggle, Toggles.TestToggle2}, null),
				new PlanTestParameters(new[] {Toggles.TestToggle, Toggles.TestToggle3}, null),
				new PlanTestParameters(new[] {Toggles.TestToggle, Toggles.TestToggle2, Toggles.TestToggle3}, null),
				new PlanTestParameters(new[] {Toggles.TestToggle2}, null),
				new PlanTestParameters(new[] {Toggles.TestToggle2, Toggles.TestToggle3}, null),
				new PlanTestParameters(new[] {Toggles.TestToggle3}, null)
			);
		}
		
		[Test]
		public void ThreeToggles_WithExtraRequest()
		{
			var target = new PlanFixtureSourceForTest(new[]{Toggles.TestToggle, Toggles.TestToggle2, Toggles.TestToggle3}, true);

			target.Should().Have.SameValuesAs(
				new PlanTestParameters(Enumerable.Empty<Toggles>(), SeperateWebRequest.SimulateFirstRequest),
				new PlanTestParameters(new[] {Toggles.TestToggle}, SeperateWebRequest.SimulateFirstRequest),
				new PlanTestParameters(new[] {Toggles.TestToggle2}, SeperateWebRequest.SimulateFirstRequest),
				new PlanTestParameters(new[] {Toggles.TestToggle3}, SeperateWebRequest.SimulateFirstRequest),
				new PlanTestParameters(new[] {Toggles.TestToggle, Toggles.TestToggle2}, SeperateWebRequest.SimulateFirstRequest),
				new PlanTestParameters(new[] {Toggles.TestToggle, Toggles.TestToggle3}, SeperateWebRequest.SimulateFirstRequest),
				new PlanTestParameters(new[] {Toggles.TestToggle2, Toggles.TestToggle3}, SeperateWebRequest.SimulateFirstRequest),
				new PlanTestParameters(new[] {Toggles.TestToggle, Toggles.TestToggle2, Toggles.TestToggle3}, SeperateWebRequest.SimulateFirstRequest),
				new PlanTestParameters(Enumerable.Empty<Toggles>(), SeperateWebRequest.SimulateSecondRequestOrScheduler),
				new PlanTestParameters(new[] {Toggles.TestToggle}, SeperateWebRequest.SimulateSecondRequestOrScheduler),
				new PlanTestParameters(new[] {Toggles.TestToggle2}, SeperateWebRequest.SimulateSecondRequestOrScheduler),
				new PlanTestParameters(new[] {Toggles.TestToggle3}, SeperateWebRequest.SimulateSecondRequestOrScheduler),
				new PlanTestParameters(new[] {Toggles.TestToggle, Toggles.TestToggle2}, SeperateWebRequest.SimulateSecondRequestOrScheduler),
				new PlanTestParameters(new[] {Toggles.TestToggle, Toggles.TestToggle3}, SeperateWebRequest.SimulateSecondRequestOrScheduler),
				new PlanTestParameters(new[] {Toggles.TestToggle2, Toggles.TestToggle3}, SeperateWebRequest.SimulateSecondRequestOrScheduler),
				new PlanTestParameters(new[] {Toggles.TestToggle, Toggles.TestToggle2, Toggles.TestToggle3}, SeperateWebRequest.SimulateSecondRequestOrScheduler)
			);
		}
		
		private class PlanFixtureSourceForTest : PlanFixtureSource
		{
			public PlanFixtureSourceForTest(IEnumerable<Toggles> toggles, bool alsoSimulateSecondRequest)
			{
				ToggleFlags = toggles;
				AlsoSimulateSecondRequest = alsoSimulateSecondRequest;
			}
			
			protected override IEnumerable<Toggles> ToggleFlags { get; }
			protected override bool AlsoSimulateSecondRequest { get; }
		}
	}
}