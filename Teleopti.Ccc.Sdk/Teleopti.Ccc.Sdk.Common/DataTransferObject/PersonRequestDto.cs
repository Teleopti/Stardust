using System;
using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject
{
    /// <summary>
    /// Represents a PersonRequestDto object.
    /// </summary>
    [DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2010/04/")]
    [KnownType(typeof(AbsenceRequestDto))]
    [KnownType(typeof(ShiftTradeRequestDto))]
    [KnownType(typeof(TextRequestDto))]
    public class PersonRequestDto : Dto
    {

        /// <summary>
        /// Gets or sets the person.
        /// </summary>
        /// <value>The person.</value>
        [DataMember]
        public PersonDto Person{ get; set; }

        /// <summary>
        /// Gets or sets the subject.
        /// </summary>
        /// <value>The subject.</value>
        [DataMember]
        public string Subject{ get; set; }

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        /// <value>The message.</value>
        [DataMember]
        public string Message{ get; set; }

        /// <summary>
        /// Gets or sets the deny reason.
        /// </summary>
        /// <value>The deny reason.</value>
        [DataMember]
        public string DenyReason { get; set; }

        /// <summary>
        /// Gets or sets the request status.
        /// </summary>
        /// <value>The request status.</value>
        [DataMember]
        public RequestStatusDto RequestStatus{ get; set; }

        /// <summary>
        /// Gets or sets the requested date.
        /// </summary>
        /// <value>The requested date.</value>
        [DataMember]
        public DateTime RequestedDate{ get; set; }

        /// <summary>
        /// Gets or sets the requested date in logged on person's time zone.
        /// </summary>
        /// <value>The requested date.</value>
        [DataMember]
        public DateTime RequestedDateLocal { get; set; }

        /// <summary>
        /// Gets or sets the created date.
        /// </summary>
        /// <value>The created date.</value>
        [DataMember]
        public DateTime CreatedDate{ get; set; }

        /// <summary>
        /// Gets or sets the requests.
        /// </summary>
        /// <value>The requests.</value>
        [DataMember]
        public RequestDto Request{ get; set; }

        /// <summary>
        /// Gets or sets an indication whether this request can be deleted.
        /// </summary>
        [DataMember]
        public bool CanDelete { get; set; }

        /// <summary>
        /// Gets or sets the updated on date and time.
        /// </summary>
        [DataMember]
        public DateTime UpdatedOn { get; set; }

        /// <summary>
        /// Gets or sets the delete flag for this instance.
        /// </summary>
        [DataMember(IsRequired = false, Order = 1)]
        public bool IsDeleted { get; set; }
    }
}