using System.Collections.Generic;
using log4net;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
	public abstract class ProjectionMerger : IProjectionMerger
	{
		private IList<IVisualLayer> _mergedCollection;
		private static readonly ILog log = LogManager.GetLogger(typeof(ProjectionMerger));

		public IList<IVisualLayer> MergedCollection(IList<IVisualLayer> unmergedCollection, IPerson person)
		{
			if (_mergedCollection == null)
			{
				var clonedUnmergedCollection = cloneUnMergedCollection(unmergedCollection, person);
				_mergedCollection = ModifyCollection(clonedUnmergedCollection);
			}
			return _mergedCollection;
		}

		protected abstract IList<IVisualLayer> ModifyCollection(IList<IVisualLayer> clonedUnmergedCollection);
		public abstract object Clone();

		private static IList<IVisualLayer> cloneUnMergedCollection(IList<IVisualLayer> unmergedCollection, IPerson person)
		{
			var layers = new List<IVisualLayer>(unmergedCollection.Count);
			foreach (var layer in unmergedCollection)
			{
				var layerClone = (IVisualLayer)layer.EntityClone();
				var casted = layerClone as VisualLayer;
				if (casted == null)
					log.Warn("Cannot cast " + layerClone + " to VisualLayer.");
				else
					casted.Person = person;
				layers.Add(layerClone);
			}
			return layers;
		}
	}
}