using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
	public abstract class ProjectionMerger : IProjectionMerger
	{
		private IEnumerable<IVisualLayer>  _mergedCollection;

		public IEnumerable<IVisualLayer>  MergedCollection(IEnumerable<IVisualLayer>  unmergedCollection)
		{
			if (_mergedCollection == null)
			{
				var clonedUnmergedCollection = cloneUnMergedCollection(unmergedCollection);
				_mergedCollection = ModifyCollection(clonedUnmergedCollection);
			}
			return _mergedCollection;
		}

		protected abstract IEnumerable<IVisualLayer> ModifyCollection(IEnumerable<IVisualLayer> clonedUnmergedCollection);
		public abstract object Clone();

		private static IEnumerable<IVisualLayer> cloneUnMergedCollection(IEnumerable<IVisualLayer>  unmergedCollection)
		{
			return unmergedCollection.Select(layer => (IVisualLayer) layer.EntityClone()).ToArray();
		}
	}
}