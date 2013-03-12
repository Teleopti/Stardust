using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
    public class PersonalShift : AggregateEntity, IPersonalShift
    {
		private IList<ILayer<IActivity>> _layerCollection = new List<ILayer<IActivity>>();

		public virtual ILayerCollection<IActivity> LayerCollection
		{
			get { return (new LayerCollection<IActivity>(this, _layerCollection)); }

		}

		public virtual IProjectionService ProjectionService()
		{
			var proj = new VisualLayerProjectionService(null);
			proj.Add(this);
			return proj;
		}

		public virtual bool HasProjection
		{
			get
			{
				return (LayerCollection.Count > 0);
			}
		}

		public virtual void Transform(IShift sourceShift)
		{
			foreach (ActivityLayer layer in sourceShift.LayerCollection)
			{
				transformOrAdd(layer);
			}

			if (sourceShift.LayerCollection.Count >= LayerCollection.Count) return;
			for (var i = LayerCollection.Count - 1; i >= 0; i--)
			{
				var layer = findLayerByOrderIndex(sourceShift.LayerCollection, LayerCollection[i].OrderIndex);

				if (layer == null)
				{
					LayerCollection.Remove(LayerCollection[i]);
				}
			}
		}

		private static ActivityLayer findLayerByOrderIndex(IEnumerable<ILayer<IActivity>> layerCollection, int orderIndex)
		{
			return layerCollection.Cast<ActivityLayer>().FirstOrDefault(layer => layer.OrderIndex == orderIndex);
		}

		private void transformOrAdd(ILayer<IActivity> sourceLayer)
		{
			var destLayer = findLayerByOrderIndex(LayerCollection, sourceLayer.OrderIndex);

			if (destLayer != null)
			{
				destLayer.Transform(sourceLayer);
			}
			else
			{
				LayerCollection.Add((ActivityLayer)sourceLayer.Clone());
			}
		}

		public virtual object Clone()
		{
			var retObj = EntityClone();
			return retObj;
		}

		public virtual IShift NoneEntityClone()
		{
			var retObj = (PersonalShift)MemberwiseClone();
			retObj.SetId(null);
			retObj._layerCollection = new List<ILayer<IActivity>>();
			foreach (var newLayer in _layerCollection.Select(layer => layer.NoneEntityClone()))
			{
				newLayer.SetParent(retObj);
				retObj._layerCollection.Add(newLayer);
			}
			return retObj;
		}

		public virtual IShift EntityClone()
		{
			var retObj = (PersonalShift)MemberwiseClone();
			retObj._layerCollection = new List<ILayer<IActivity>>();
			foreach (var newLayer in _layerCollection.Select(layer => layer.EntityClone()))
			{
				newLayer.SetParent(retObj);
				retObj._layerCollection.Add(newLayer);
			}
			return retObj;
		}

		public virtual IVisualLayerFactory CreateVisualLayerFactory()
		{
			return new VisualLayerFactory();
		}

        public virtual int OrderIndex
        {
            get
            {
                if (Parent == null)
                    return -1;
                return ((IPersonAssignment) Parent).PersonalShiftCollection.IndexOf(this);
            }
        }

        public virtual void OnAdd(ILayer<IActivity> layer)
        {
            if (!(layer is PersonalShiftActivityLayer))
                throw new ArgumentException("Only PersonalShiftActivityLayers can be added to a PersonalShift");
        }
    }
}