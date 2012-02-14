using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
    public class PersonalShift : Shift, IPersonalShift
    {
        public virtual int OrderIndex
        {
            get
            {
                if (Parent == null)
                    return -1;
                return ((IPersonAssignment) Parent).PersonalShiftCollection.IndexOf(this);
            }
        }

        public override void OnAdd(ILayer<IActivity> layer)
        {
            if (!(layer is PersonalShiftActivityLayer))
                throw new ArgumentException("Only PersonalShiftActivityLayers can be added to a PersonalShift");
        }
    }
}