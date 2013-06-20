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

		public virtual object Clone()
		{
			return EntityClone();
		}

		public virtual IPersonalShift NoneEntityClone()
		{
			var retObj = (PersonalShift)MemberwiseClone();
			retObj.SetId(null);
			retObj._layerCollection = new List<ILayer<IActivity>>();
			foreach (IPersistedLayer<IActivity> newLayer in _layerCollection.Select(layer => layer.NoneEntityClone()))
			{
				newLayer.SetParent(retObj);
				retObj._layerCollection.Add(newLayer);
			}
			return retObj;
		}

		public virtual IPersonalShift EntityClone()
		{
			var retObj = (PersonalShift)MemberwiseClone();
			retObj._layerCollection = new List<ILayer<IActivity>>();
			foreach (IPersistedLayer<IActivity> newLayer in _layerCollection.Select(layer => layer.EntityClone()))
			{
				newLayer.SetParent(retObj);
				retObj._layerCollection.Add(newLayer);
			}
			return retObj;
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