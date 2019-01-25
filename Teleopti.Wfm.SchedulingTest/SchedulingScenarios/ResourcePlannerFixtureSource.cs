using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.FeatureFlags;

namespace Teleopti.Wfm.SchedulingTest.SchedulingScenarios
{
	public abstract class ResourcePlannerFixtureSource : IEnumerable<ResourcePlannerTestParameters>
	{
		protected abstract IEnumerable<Toggles> ToggleFlags { get; }
		protected abstract bool AlsoSimulateSecondRequest { get; }
		
		public IEnumerator<ResourcePlannerTestParameters> GetEnumerator()
		{
			var toggleCombos = new HashSet<IEnumerable<Toggles>>(new togglesComparer()) {Enumerable.Empty<Toggles>()};
			foreach (var toggleOuter in ToggleFlags)
			{
				innerEnumerator(toggleOuter, toggleCombos);
			}

			var testParameters = new List<ResourcePlannerTestParameters>();
			foreach (var toggleCombo in toggleCombos)
			{
				if (AlsoSimulateSecondRequest)
				{
					testParameters.Add(new ResourcePlannerTestParameters(toggleCombo, SeparateWebRequest.SimulateFirstRequest));
					testParameters.Add(new ResourcePlannerTestParameters(toggleCombo, SeparateWebRequest.SimulateSecondRequestOrScheduler));
				}
				else
				{
					testParameters.Add(new ResourcePlannerTestParameters(toggleCombo, null));
				}
			}
			testParameters.Sort();
			return testParameters.GetEnumerator();
		}

		private void innerEnumerator(Toggles mainToggle, ISet<IEnumerable<Toggles>> toggleCombos, int startPos = 0)
		{
			var allToggles = ToggleFlags.ToArray();
			var toggleItems = new List<Toggles> {mainToggle};
			for (var i = startPos; i < allToggles.Length; i++)
			{
				toggleItems.Add(allToggles[i]);
				toggleCombos.Add(new HashSet<Toggles>(toggleItems));
				innerEnumerator(mainToggle, toggleCombos, i + 1);
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
		
		private class togglesComparer : IEqualityComparer<IEnumerable<Toggles>>
		{
			public bool Equals(IEnumerable<Toggles> x, IEnumerable<Toggles> y)
			{
				return x.All(y.Contains);
			}

			public int GetHashCode(IEnumerable<Toggles> obj)
			{
				return obj.Count();
			}
		}
	}
}