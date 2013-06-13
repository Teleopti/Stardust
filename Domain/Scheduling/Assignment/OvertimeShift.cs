using System.Collections.Generic;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Interfaces.Domain;
using System;
using System.Linq;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
	public class OvertimeShift : AggregateEntity, IOvertimeShift
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

		public virtual object Clone()
		{
			var retObj = EntityClone();
			return retObj;
		}

		public virtual IShift NoneEntityClone()
		{
			var retObj = (OvertimeShift)MemberwiseClone();
			retObj.SetId(null);
			retObj._layerCollection = new List<ILayer<IActivity>>();
			foreach (IPersistedLayer<IActivity> newLayer in _layerCollection.Select(layer => layer.NoneEntityClone()))
			{
				newLayer.SetParent(retObj);
				retObj._layerCollection.Add(newLayer);
			}
			return retObj;
		}

		public virtual IShift EntityClone()
		{
			var retObj = (OvertimeShift)MemberwiseClone();
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
                return ((IPersonAssignment)Parent).OvertimeShiftCollection.IndexOf(this);
            }
        }

		public virtual void OnAdd(ILayer<IActivity> layer)
		{
			if (!(layer is OvertimeShiftActivityLayer))
				throw new ArgumentException("Only OverTimeShiftActivityLayers can be added to a OvertimeShift");
			if (Parent == null)
				throw new InvalidOperationException("Cannot add layer to overtime shift before shift is connected to assignment.");

			var ass = ((IPersonAssignment)Root());
			var timeZoneInfo = ass.Person.PermissionInformation.DefaultTimeZone();
			var dateOnlyPerson = new DateOnly(TimeZoneHelper.ConvertFromUtc(layer.Period.StartDateTime, timeZoneInfo).Date);
			var period = ass.Person.Period(dateOnlyPerson);
			if (period != null)
			{
				var castedLayer = (IOvertimeShiftActivityLayer)layer;
				if (period.PersonContract.Contract.MultiplicatorDefinitionSetCollection.Contains(castedLayer.DefinitionSet))
					return;
			}
			throw new ArgumentException("Trying to add an overtimeactivitylayer with a definition set not defined for person's period.");
		}

        public virtual IEnumerable<IOvertimeShiftActivityLayer> LayerCollectionWithDefinitionSet()
        {
            return LayerCollection.Cast<IOvertimeShiftActivityLayer>();
        }

        public virtual IVisualLayerFactory CreateVisualLayerFactory()
        {
            return new VisualLayerOvertimeFactory();
        }
    }
}
