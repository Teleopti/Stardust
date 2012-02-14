using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject
{
    /// <summary>
    /// A dialogue message.
    /// </summary>
    [DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2010/04/")]
    public class DialogueMessageDto : Dto
    {
        /// <summary>
        /// Gets or sets the sender of the message.
        /// </summary>
        [DataMember]
        public PersonDto Sender { get; set; }

        /// <summary>
        /// Gets or sets the message text.
        /// </summary>
        [DataMember]
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets the created on details (date of creation).
        /// </summary>
        [DataMember]
        public string CreatedOn { get; set; }
    }
}