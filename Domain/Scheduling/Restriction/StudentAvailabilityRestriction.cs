using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Restriction
{
    

    public class StudentAvailabilityRestriction : RestrictionBase, IStudentAvailabilityRestriction
    {
        
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "value"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public virtual int Index
        {
            get
            {
                if (Parent != null)
                {
                    return ((StudentAvailabilityDay)Parent).IndexInCollection(this);
                }
                return -1;
            }
        }

        public virtual IStudentAvailabilityRestriction NoneEntityClone()
        {
            IStudentAvailabilityRestriction ret = (IStudentAvailabilityRestriction)Clone();
            ret.SetId(null);

            return ret;
        }

        public virtual IStudentAvailabilityRestriction EntityClone()
        {
            return (IStudentAvailabilityRestriction)Clone();    
        }
    }
}