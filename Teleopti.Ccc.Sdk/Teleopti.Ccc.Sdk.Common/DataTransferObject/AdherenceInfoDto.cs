using System;
using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject
{
    [Serializable]
    [DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2010/04/")]
    public class AdherenceInfoDto
    {
        private DateOnlyDto _dateOnlyDto;
        private long _scheduleWorkCtiTime;
        private long _loggedInTime;
        private long _idleTime;
        private long _availableTime;

        [DataMember]
        public DateOnlyDto DateOnlyDto
        {
            get { return _dateOnlyDto; }
            set { _dateOnlyDto = value; }
        }

        /// <summary>
        /// Gets or sets the schedule work cti time. Ticks!
        /// </summary>
        /// <value>The schedule work cti time.</value>
        [DataMember]
        public long ScheduleWorkCtiTime
        {
            get { return _scheduleWorkCtiTime; }
            set { _scheduleWorkCtiTime = value; }
        }

        /// <summary>
        /// Gets or sets the logged in time. Ticks!
        /// </summary>
        /// <value>The logged in time.</value>
        [DataMember]
        public long LoggedInTime
        {
            get { return _loggedInTime; }
            set { _loggedInTime = value; }
        }

        /// <summary>
        /// Gets or sets the idle time. Ticks!
        /// </summary>
        /// <value>The idle time.</value>
        [DataMember]
        public long IdleTime
        {
            get { return _idleTime; }
            set { _idleTime = value; }
        }

        /// <summary>
        /// Gets or sets the available time. Ticks!
        /// </summary>
        /// <value>The available time.</value>
        [DataMember]
        public long AvailableTime
        {
            get { return _availableTime; }
            set { _availableTime = value; }
        }
    }
}
