using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos
{
    /// <summary>
    /// Specify a query to get team by name.
    /// </summary>
    [DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2011/07/")]
    public class GetValidatedSchedulePartsForStudentAvailabilityQueryDto:QueryDto
    {
        /// <summary>
        /// Gets and sets person.
        /// </summary>
        /// <value>The person.</value>
        [DataMember]
        public PersonDto Person
        {
            get;
            set;
        }

        /// <summary>
        /// Gets and sets date in period.
        /// </summary>
        /// <value>The date in period.</value>
        [DataMember]
        public DateOnlyDto DateInPeriod
        {
            get;
            set;
        }

        /// <summary>
        /// Gets and sets time zone id.
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
