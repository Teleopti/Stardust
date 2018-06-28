using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
	public class NoProjectionMerger : IProjectionMerger
	{
		public object Clone()
		{
			return new NoProjectionMerger();
		}

		public IVisualLayer[] MergedCollection(IVisualLayer[] unmergedCollection)
		{
			return unmergedCollection;
		}
	}
}