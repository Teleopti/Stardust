using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject
{
    /// <summary>
    /// The base class containing common properties for all types of restrictions.
    /// </summary>
    /// <remarks>Examples of restrictions in the system are preferences and student availability.</remarks>
    [DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2010/04/")]
    [KnownType(typeof(PreferenceRestrictionDto))]
    [KnownType(typeof(StudentAvailabilityRestrictionDto))]
    public class RestrictionBaseDto : Dto
    {
        /// <summary>
        /// Gets or sets the details for start time limitations.
        /// </summary>
        [DataMember]
        public TimeLimitationDto StartTimeLimitation { get; set; }

        /// <summary>
        /// Gets or sets the details for end time limitations.
        /// </summary>
        [DataMember]
        public TimeLimitationDto EndTimeLimitation { get; set; }

        /// <summary>
        /// Gets or sets the details for work time limitations.
        /// </summary>
        [DataMember]
        public TimeLimitationDto WorkTimeLimitation { get; set; }

        /// <summary>
        /// Gets or sets the start time limitations as text.
        /// </summary>
        [DataMember]
        public string LimitationStartTimeString { get; set; }

        /// <summary>
        /// Gets or sets the end time limitations as text.
        /// </summary>
        [DataMember]
        public string LimitationEndTimeString { get; set; }
    }
}