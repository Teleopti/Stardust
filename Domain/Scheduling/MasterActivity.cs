using System.Collections.Generic;
using Teleopti.Interfaces.Domain;


namespace Teleopti.Ccc.Domain.Scheduling
{
    public class MasterActivity: Activity, IMasterActivity
    {
        private IList<IActivity> _activityCollection = new List<IActivity>();

        public virtual IList<IActivity> ActivityCollection
        {
            get { return _activityCollection; }
        }

        public virtual void UpdateActivityCollection(IList<IActivity> newActivityCollection)
        {
            _activityCollection = newActivityCollection;
        }

        public override bool InContractTime
        {
            get
            {
                return true;
            }
            set
            {
                base.InContractTime = value;
            }
        }
       
    }
}