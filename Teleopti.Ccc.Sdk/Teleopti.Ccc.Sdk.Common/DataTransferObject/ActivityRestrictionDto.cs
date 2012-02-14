using System;
using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject
{
    /// <summary>
    /// A restriction for a single activity. For example an extended preference for activity Lunch.
    /// </summary>
    [DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2010/05/")]
    public class ActivityRestrictionDto:Dto
    {
        /// <summary>
        /// The preference for end time of the specified activity.
        /// </summary>
        [DataMember]
        public TimeLimitationDto EndTimeLimitation { get; set; }

        /// <summary>
        /// The preference for start time of the specified activity.
        /// </summary>
        [DataMember]
        public TimeLimitationDto StartTimeLimitation { get; set; }

        /// <summary>
        /// The preference for work time of the specified activity.
        /// </summary>
        [DataMember]
        public TimeLimitationDto WorkTimeLimitation { get; set; }

        /// <summary>
        /// The activity for this preference.
        /// </summary>
        [DataMember]
        public ActivityDto Activity { get; set; }
    }
}