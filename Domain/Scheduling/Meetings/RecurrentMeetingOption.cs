using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Meetings
{
    /// <summary>
    /// Represents the recurrent option of a meeting
    /// </summary>
    public abstract class RecurrentMeetingOption : AggregateEntity, IRecurrentMeetingOption
    {
        private int _incrementCount = 1;

        public virtual int IncrementCount
        {
            get
            {
                return _incrementCount;
            }
            set
            {
                InParameter.ValueMustBeLargerThanZero(nameof(IncrementCount), value);
                _incrementCount = value;
            }
        }

        public abstract IList<DateOnly> GetMeetingDays(DateOnly startDate, DateOnly endDate);

        public virtual object Clone()
        {
            IRecurrentMeetingOption retObj = EntityClone();
            AddExtraCloneData(retObj);
            return retObj;
        }

        protected virtual void AddExtraCloneData(IRecurrentMeetingOption retObj)
        {
        }

        public virtual IRecurrentMeetingOption NoneEntityClone()
        {
            var retObj = (RecurrentMeetingOption)MemberwiseClone();
            AddExtraCloneData(retObj);
            retObj.SetId(null);
            return retObj;
        }
        
        public virtual IRecurrentMeetingOption EntityClone()
        {
            var retObj = (RecurrentMeetingOption)MemberwiseClone();
            AddExtraCloneData(retObj);
            return retObj;
        }
    }
}
