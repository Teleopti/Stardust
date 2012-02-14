using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Interfaces.Domain;
using Teleopti.Ccc.Domain.Collection;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
    /// <summary>
    /// Base class for shifts
    /// </summary>
    public abstract class Shift : AggregateEntity, IShift
    {
        private IList<ILayer<IActivity>> _layerCollection = new List<ILayer<IActivity>>();

        /// <summary>
        /// Gets the layer collection.
        /// </summary>
        /// <value>The layer collection.</value>
        public virtual ILayerCollection<IActivity> LayerCollection
        {
            get { return (new LayerCollection<IActivity>(this, _layerCollection)); }

        }

        #region IProjection<Activity> Members

        /// <summary>
        /// Creates the projection.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-01-28
        /// </remarks>
        public virtual IProjectionService ProjectionService()
        {
            VisualLayerProjectionService proj = new VisualLayerProjectionService(null);
            proj.Add(this);
            return proj;
        }

       
        /// <summary>
        /// Gets a value indicating whether this instance has projection.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance has projection; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-01-28
        /// </remarks>
        public virtual bool HasProjection
        {
            get
            {
                return (LayerCollection.Count > 0);
            }
        }

        #endregion

        /// <summary>
        /// Called before layer is added to collection.
        /// </summary>
        /// <param name="layer">The layer.</param>
        /// <remarks>
        /// Check here on shift because we want activity layers to be persisted in different tables
        /// (eg adding an activity layer to a MainShift shouldn't be possible even though it makes
        /// perfect sence regarding objects)
        /// Created by: rogerkr
        /// Created date: 2008-01-25
        /// </remarks>
        public abstract void OnAdd(ILayer<IActivity> layer);

        

        /// <summary>
        /// Transform one shift(this instance) to another shift
        /// </summary>
        /// <param name="sourceShift"></param>
        public virtual void Transform(IShift sourceShift)
        {
            foreach (ActivityLayer layer in sourceShift.LayerCollection)
            {
                TransformOrAdd(layer);
            }

            if (sourceShift.LayerCollection.Count < LayerCollection.Count)
            {
                for (int i = LayerCollection.Count - 1; i >= 0; i--)
                {
                    ActivityLayer layer = FindLayerByOrderIndex(sourceShift.LayerCollection, LayerCollection[i].OrderIndex);

                    if (layer == null)
                    {
                        LayerCollection.Remove(LayerCollection[i]);
                    }
                }
            }

        }

        private static ActivityLayer FindLayerByOrderIndex(IEnumerable<ILayer<IActivity>> layerCollection, int orderIndex)
        {
            ActivityLayer destLayer = null;

            foreach (ActivityLayer layer in layerCollection)
            {
                if (layer.OrderIndex == orderIndex)
                {
                    destLayer = layer;
                    break;
                }
            }

            return destLayer;
        }

        private void TransformOrAdd(ILayer<IActivity> sourceLayer)
        {
            ActivityLayer destLayer = FindLayerByOrderIndex(LayerCollection, sourceLayer.OrderIndex);

            if (destLayer != null)
            {
                destLayer.Transform(sourceLayer);
            }
            else
            {
                LayerCollection.Add((ActivityLayer)sourceLayer.Clone());
            }
        }

        #region ICloneableEntity<Shift> Members

        #region ICloneable Members

        ///<summary>
        ///Creates a new object that is a copy of the current instance.
        ///</summary>
        ///
        ///<returns>
        ///A new object that is a copy of this instance.
        ///</returns>
        public virtual object Clone()
        {
            IShift retObj = EntityClone();         
            return retObj;
        }

        #endregion

        /// <summary>
        /// Returns a clone of this T with IEntitiy.Id set to null.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-05-27
        /// </remarks>
        public virtual IShift NoneEntityClone()
        {
            Shift retObj = (Shift)MemberwiseClone();
            retObj.SetId(null);
            retObj._layerCollection = new List<ILayer<IActivity>>();
            foreach (ILayer<IActivity> layer in _layerCollection)
            {
                ILayer<IActivity> newLayer = layer.NoneEntityClone();
                newLayer.SetParent(retObj);
                retObj._layerCollection.Add(newLayer);
            }
            return retObj;
        }

        /// <summary>
        /// Returns a clone of this T with IEntitiy.Id as this T.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-05-27
        /// </remarks>
        public virtual IShift EntityClone()
        {
            Shift retObj = (Shift)MemberwiseClone();
            retObj._layerCollection = new List<ILayer<IActivity>>();
            foreach (ILayer<IActivity> layer in _layerCollection)
            {
                ILayer<IActivity> newLayer = layer.EntityClone();
                newLayer.SetParent(retObj);
                retObj._layerCollection.Add(newLayer);
            }
            return retObj;
        }

        #endregion

        public virtual IVisualLayerFactory CreateVisualLayerFactory()
        {
            return new VisualLayerFactory();
        }
    }
}
