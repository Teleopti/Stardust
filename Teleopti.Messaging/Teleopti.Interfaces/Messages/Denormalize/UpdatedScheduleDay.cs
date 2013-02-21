using System;

namespace Teleopti.Interfaces.Messages.Denormalize
{
    /// <summary>
    /// 
    /// </summary>
    public class UpdatedScheduleDay : RaptorDomainMessage
    {
        private readonly Guid _messageId = Guid.NewGuid();
        private Guid _personId;
        private DateTime _activityStartDateTime;
        private DateTime _activityEndDateTime;

        ///<summary>
        /// Definies an identity for this message (typically the Id of the root this message refers to.
        ///</summary>
        public override Guid Identity
        {
            get { return _messageId; }
        }

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
