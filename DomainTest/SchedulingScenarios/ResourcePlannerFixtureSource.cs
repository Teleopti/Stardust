using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.FeatureFlags;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios
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

			var sortedTestParameters = new List<ResourcePlannerTestParameters>();
			var sortedToggleCombos = toggleCombos.OrderBy(x => x.Count()).ThenBy(x => x.FirstOrDefault().ToString()).ToList();

			foreach (var toggleCombo in sortedToggleCombos)
			{
				sortedTestParameters.Add(AlsoSimulateSecondRequest ? 
					new ResourcePlannerTestParameters(toggleCombo, SeperateWebRequest.SimulateFirstRequest) : 
					new ResourcePlannerTestParameters(toggleCombo, null));
			}

			if (AlsoSimulateSecondRequest)
			{
				foreach (var toggleCombo in sortedToggleCombos)
				{
					sortedTestParameters.Add(new ResourcePlannerTestParameters(toggleCombo, SeperateWebRequest.SimulateSecondRequestOrScheduler));	
				}
			}
			
			return sortedTestParameters.GetEnumerator();
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