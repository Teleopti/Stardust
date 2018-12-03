using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Domain.Scheduling.ScheduleTagging
{
    public class ScheduleTag : VersionedAggregateRootWithBusinessUnit, IScheduleTag, IDeleteTag
    {
        private bool _isDeleted;
        private string _description;

        public virtual bool IsDeleted => _isDeleted;

	    public virtual void SetDeleted()
        {
            _isDeleted = true;
        }

        public virtual string Description
        {
            get { return _description; }
            set 
            { 
                InParameter.StringTooLong(nameof(Description), value, 15);
                _description = value;
            }
        }
    }
}