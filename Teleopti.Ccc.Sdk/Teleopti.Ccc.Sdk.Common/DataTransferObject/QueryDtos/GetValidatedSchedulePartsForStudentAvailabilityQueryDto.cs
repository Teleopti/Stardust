using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos
{
	/// <summary>
	/// Query to get a collection of <see cref="ValidatedSchedulePartDto"/> for the given person and schedule period.
	/// </summary>
	/// <remarks>Used to display student availability details.</remarks>
    [DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2011/07/")]
    public class GetValidatedSchedulePartsForStudentAvailabilityQueryDto:QueryDto
    {
        /// <summary>
        /// Gets and sets the mandatory person.
        /// </summary>
        /// <value>The person.</value>
        [DataMember]
        public PersonDto Person
        {
            get;
            set;
        }

        /// <summary>
        /// Gets and sets the mandatory date in period.
        /// </summary>
        /// <value>The date in period.</value>
        [DataMember]
        public DateOnlyDto DateInPeriod
        {
            get;
            set;
        }

        /// <summary>
        /// Gets and sets the mandatory time zone id.
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
