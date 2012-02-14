using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Restriction
{
   
    public class AvailabilityRestriction : RestrictionBase, IAvailabilityRestriction
    {
        private bool _notAvailable;

        public virtual bool NotAvailable
        {
            get { return _notAvailable; }
            set { _notAvailable = value; }
        }

        public override bool IsRestriction()
        {          
            if (NotAvailable)
                return true;
            return base.IsRestriction();
        }

    }
}