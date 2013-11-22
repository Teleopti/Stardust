using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.Scheduling.ScheduleTagging
{
    public class ScheduleTag : VersionedAggregateRootWithBusinessUnit, IScheduleTag, IDeleteTag
    {
        private bool _isDeleted;
        private string _description;

        public virtual bool IsDeleted
        {
            get { return _isDeleted; }
        }

        public virtual void SetDeleted()
        {
            _isDeleted = true;
        }

        public virtual string Description
        {
            get { return _description; }
            set 
            { 
                InParameter.StringTooLong("Description", value, 15);
                _description = value;
            }
        }
    }
}