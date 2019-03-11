using System;
using System.Collections.Generic;
using System.Linq;

namespace Teleopti.Ccc.Domain.Status
{
	public class AllSteps
	{
		private readonly IEnumerable<IStatusStep> _fixedStatusSteps;
		private readonly IFetchCustomStatusSteps _fetchCustomStatusSteps;

		public AllSteps(IEnumerable<IStatusStep> fixedStatusSteps, IFetchCustomStatusSteps fetchCustomStatusSteps)
		{
			_fixedStatusSteps = fixedStatusSteps;
			_fetchCustomStatusSteps = fetchCustomStatusSteps;
		}

		public IEnumerable<IStatusStep> FetchAll()
		{
			return _fetchCustomStatusSteps.Execute().Union(_fixedStatusSteps).ToArray();
		}

		public IStatusStep Fetch(string stepName)
		{
			//TODO: optimize db calls if necessary - let's start loading them all each time...
			return FetchAll().SingleOrDefault(x => string.Equals(x.Name, stepName, StringComparison.InvariantCultureIgnoreCase));
		}
	}
}