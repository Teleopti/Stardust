using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
	public class NoProjectionMerger : IProjectionMerger
	{
		public object Clone()
		{
			return new NoProjectionMerger();
		}

		public IEnumerable<IVisualLayer> MergedCollection(IEnumerable<IVisualLayer> unmergedCollection)
		{
			return unmergedCollection;
		}
	}
}