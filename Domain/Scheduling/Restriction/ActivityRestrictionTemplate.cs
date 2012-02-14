using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Restriction
{
    public class ActivityRestrictionTemplate : ActivityRestriction, IActivityRestrictionTemplate
    {
        public ActivityRestrictionTemplate(IActivity activity) : base(activity)
        {
        }
        
        protected ActivityRestrictionTemplate()
        {}
    }
}