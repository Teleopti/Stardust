using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
	public abstract class ProjectionMerger : IProjectionMerger
	{
		private IVisualLayer[] _mergedCollection;

		public IVisualLayer[] MergedCollection(IVisualLayer[] unmergedCollection)
		{
			if (_mergedCollection == null)
			{
				var clonedUnmergedCollection = cloneUnMergedCollection(unmergedCollection);
				_mergedCollection = ModifyCollection(clonedUnmergedCollection);
			}
			return _mergedCollection;
		}

		protected abstract IVisualLayer[] ModifyCollection(IVisualLayer[] clonedUnmergedCollection);
		public abstract object Clone();

		private static IVisualLayer[] cloneUnMergedCollection(IVisualLayer[] unmergedCollection)
		{
			return unmergedCollection.Select(layer => (IVisualLayer) layer.EntityClone()).ToArray();
		}
	}
}