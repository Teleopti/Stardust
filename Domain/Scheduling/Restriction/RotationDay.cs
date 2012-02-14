using System.Collections.Generic;
using System.Collections.ObjectModel;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Restriction
{ 
    public class RotationDay : AggregateEntity, IRotationDay
    {
        private readonly IList<IRotationRestriction> _restrictionCollection = new List<IRotationRestriction>();

        public RotationDay()
        {
            RotationRestriction restriction = new RotationRestriction();
            restriction.SetParent(this);
            _restrictionCollection.Add(restriction);
        }

        public virtual ReadOnlyCollection<IRotationRestriction> RestrictionCollection
        {
            get {return new ReadOnlyCollection<IRotationRestriction>(_restrictionCollection);}
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "value"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public virtual int Index
        {
            get
            {
                if (Parent != null)
                {
                     return ((Rotation)Parent).RotationDays.IndexOf(this);
                }
                return -1;
            }
        }

        public virtual IRotationRestriction SignificantRestriction()
        {
            return _restrictionCollection[0];
        }

        public virtual bool IsRotationDay()
        {
            return SignificantRestriction().IsRestriction();
        }
    }
}