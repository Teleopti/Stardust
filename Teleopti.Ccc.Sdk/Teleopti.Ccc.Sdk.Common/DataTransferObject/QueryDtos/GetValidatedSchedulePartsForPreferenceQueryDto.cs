using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos
{
    /// <summary>
    /// Query to get a collection of <see cref="ValidatedSchedulePartDto"/> for the given person and schedule period.
    /// </summary>
    /// <remarks>Used to display preference details.</remarks>
    [DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2011/07/")]
    public class GetValidatedSchedulePartsForPreferenceQueryDto:QueryDto
    {
        /// <summary>
        /// Gets and sets mandatory person.
        /// </summary>
        /// <value>The person.</value>
        [DataMember]
        public PersonDto Person
        {
            get;
            set;
        }

        /// <summary>
        /// Gets and sets mandatory date in period.
        /// </summary>
        /// <value>The date in period.</value>
        /// <remarks>This should be a single date within the schedule period of interest.</remarks>
        [DataMember]
        public DateOnlyDto DateInPeriod
        {
            get;
            set;
        }

        /// <summary>
        /// Gets and sets mandatory time zone id.
        /// </summary>
        /// <value>The time zone id.</value>
        [DataMember]
        public string TimeZoneId
        {
            get;
            set;
        }
    }
}
