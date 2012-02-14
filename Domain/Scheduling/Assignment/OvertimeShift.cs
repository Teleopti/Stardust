using System.Collections.Generic;
using Teleopti.Interfaces.Domain;
using System;
using System.Linq;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
    public class OvertimeShift : Shift, IOvertimeShift
    {
        public virtual int OrderIndex
        {
            get
            {
                if (Parent == null)
                    return -1;
                return ((IPersonAssignment)Parent).OvertimeShiftCollection.IndexOf(this);
            }
        }

        public override void OnAdd(ILayer<IActivity> layer)
        {
            if (!(layer is OvertimeShiftActivityLayer))
                throw new ArgumentException("Only OverTimeShiftActivityLayers can be added to a OvertimeShift");
            if(Parent==null)
                throw new InvalidOperationException("Cannot add layer to overtime shift before shift is connected to assignment.");

            IPersonAssignment ass = ((IPersonAssignment) Root());
            ICccTimeZoneInfo timeZoneInfo = ass.Person.PermissionInformation.DefaultTimeZone();
            DateOnly dateOnlyPerson = new DateOnly(TimeZoneHelper.ConvertFromUtc(layer.Period.StartDateTime, timeZoneInfo).Date);
            IPersonPeriod period = ass.Person.Period(dateOnlyPerson);
            if(period!=null)
            {
                IOvertimeShiftActivityLayer castedLayer = (IOvertimeShiftActivityLayer) layer;
                if(period.PersonContract.Contract.MultiplicatorDefinitionSetCollection.Contains(castedLayer.DefinitionSet))
                    return;
            }
            throw new ArgumentException("Trying to add an overtimeactivitylayer with a definition set not defined for person's period.");
        }

        public virtual IEnumerable<IOvertimeShiftActivityLayer> LayerCollectionWithDefinitionSet()
        {
            return LayerCollection.Cast<IOvertimeShiftActivityLayer>();
        }

        public override IVisualLayerFactory CreateVisualLayerFactory()
        {
            return new VisualLayerOvertimeFactory();
        }
    }
}
