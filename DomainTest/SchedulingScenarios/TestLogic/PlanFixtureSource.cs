using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.FeatureFlags;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.TestLogic
{
	public abstract class PlanFixtureSource : IEnumerable<PlanTestParameters>
	{
		protected abstract IEnumerable<Toggles> ToggleFlags { get; }
		protected abstract bool AlsoSimulateSecondRequest { get; }
		
		public IEnumerator<PlanTestParameters> GetEnumerator()
		{
			//put logic here
			return Enumerable.Empty<PlanTestParameters>().GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}