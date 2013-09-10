using System;
using Teleopti.Ccc.Domain.ApplicationLayer;

namespace Teleopti.Ccc.Domain.ApplicationRtaQueue
{
    /// <summary>
    /// 
    /// </summary>
    public class UpdatedScheduleDay : RaptorDomainEvent
    {
        private Guid _personId;
        private DateTime _activityStartDateTime;
        private DateTime _activityEndDateTime;

        /// <summary>
        /// 
        /// </summary>
        public Guid PersonId
        {
            get { return _personId; }
            set { _personId = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public DateTime ActivityStartDateTime
        {
            get { return _activityStartDateTime; }
            set { _activityStartDateTime = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public DateTime ActivityEndDateTime
        {
            get { return _activityEndDateTime; }
            set { _activityEndDateTime = value; }
        }

    }
}
