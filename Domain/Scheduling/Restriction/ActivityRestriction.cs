using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Restriction
{
    public class ActivityRestriction : RestrictionBase, IActivityRestriction
    {
        private IActivity _activity;

        public ActivityRestriction(IActivity activity)
        {
            _activity = activity;
        }
        
        protected ActivityRestriction()
        {}

        public virtual IActivity Activity
        {
            get { return _activity; }
            set { _activity = value; }
        }
        
        public virtual IActivityRestriction NoneEntityClone()
        {
            var ret = (IActivityRestriction)Clone();
            ret.SetId(null);
            return ret;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var result = 0;
                result = (result * 399) ^ base.GetHashCode();
                result = (result * 399) ^ _activity.GetHashCode();
                return result;
            }
        }
    }
}