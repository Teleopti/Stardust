using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject
{
    /// <summary>
    /// Represents a Public Note object for a schedule day
    /// </summary>
    [DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2011/01/")]
    public class PublicNoteDto : Dto
    {
        /// <summary>
        /// Gets or sets the person.
        /// </summary>
        /// <value>The person.</value>
        [DataMember]
        public PersonDto Person { get; set; }

        /// <summary>
        /// Gets or sets the schedule note.
        /// </summary>
        /// <value>The schedule note.</value>
        [DataMember]
        public string ScheduleNote { get; set; }

        /// <summary>
        /// Gets or sets the date.
        /// </summary>
        /// <value>The date.</value>
        [DataMember]
        public DateOnlyDto Date { get; set; }
    }
}