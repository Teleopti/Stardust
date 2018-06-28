using System.Collections.Generic;
using log4net;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
	public abstract class ProjectionMerger : IProjectionMerger
	{
		private IVisualLayer[] _mergedCollection;
		private static readonly ILog log = LogManager.GetLogger(typeof(ProjectionMerger));

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
			var layers = new List<IVisualLayer>(unmergedCollection.Length);
			foreach (var layer in unmergedCollection)
			{
				var layerClone = (IVisualLayer)layer.EntityClone();
				var casted = layerClone as VisualLayer;
				if (casted == null)
					log.Warn("Cannot cast " + layerClone + " to VisualLayer.");
				layers.Add(layerClone);
			}
			return layers.ToArray();
		}
	}
}