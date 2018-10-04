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
			var toggleCombos = new HashSet<IEnumerable<Toggles>> {Enumerable.Empty<Toggles>()};
			foreach (var toggleOuter in ToggleFlags)
			{
				innerEnumerator(toggleOuter, toggleCombos);
			}

			foreach (var toggleCombo in toggleCombos)
			{
				if (AlsoSimulateSecondRequest)
				{
					yield return new ResourcePlannerTestParameters(toggleCombo, SeperateWebRequest.SimulateFirstRequest);
					yield return new ResourcePlannerTestParameters(toggleCombo, SeperateWebRequest.SimulateSecondRequestOrScheduler);
				}
				else
				{
					yield return new ResourcePlannerTestParameters(toggleCombo, null);					
				}
			}
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
	}
}