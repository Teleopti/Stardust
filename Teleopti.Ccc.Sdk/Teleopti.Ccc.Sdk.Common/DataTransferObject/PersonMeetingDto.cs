using System;
using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject
{
    /// <summary>
    /// Represents a PersonMeetingDto object.
    /// </summary>
    [DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2010/04/")]
    public class PersonMeetingDto : LayerDto
    {
        /// <summary>
        /// Gets or sets the person.
        /// </summary>
        /// <value>The person.</value>
        [DataMember]
        public PersonDto Person { get; set; }

        /// <summary>
        /// Gets or sets the subject.
        /// </summary>
        /// <value>The subject.</value>
        [DataMember]
        public string Subject { get; set; }

        /// <summary>
        /// Gets or sets the location.
        /// </summary>
        /// <value>The location.</value>
        [DataMember]
        public string Location { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>The description.</value>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the meeting id.
        /// </summary>
        /// <value>The meeting id.</value>
        [DataMember]
        public Guid MeetingId { get; set; }
    }
}