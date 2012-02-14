using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Restriction
{
    public class AvailabilityDay : AggregateEntity, IAvailabilityDay
    {
        private IAvailabilityRestriction _restriction;
        public AvailabilityDay()
        {
            _restriction = new AvailabilityRestriction();
            _restriction.SetParent(this);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "value"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public virtual int Index
        {
            get
            {
                if (Parent != null)
                {
                    return ((AvailabilityRotation)Parent).AvailabilityDays.IndexOf(this);
                }
                return -1;
            }
        }

        
        public virtual IAvailabilityRestriction Restriction
        {
            get { return _restriction; }
            set
            {
                _restriction = value;
                if (_restriction != null)
                    _restriction.SetParent(this);
            }
        }

        public virtual bool IsAvailabilityDay()
        {
            return Restriction.IsRestriction();
        }
    }
}