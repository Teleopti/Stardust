using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject
{
    /// <summary>
    /// A complete dialogue of messages.
    /// </summary>
    [DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2010/04/")]
    public class PushMessageDialogueDto : Dto
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PushMessageDialogueDto"/> class.
        /// </summary>
        public PushMessageDialogueDto()
        {
            Messages = new List<DialogueMessageDto>();
        }

        /// <summary>
        /// Gets or sets an indication if this message was replied to.
        /// </summary>
        [DataMember]
        public bool IsReplied { get; set; }

        /// <summary>
        /// Gets or sets the inital message of this dialogue.
        /// </summary>
        [DataMember]
        public PushMessageDto PushMessage { get; set; }

        /// <summary>
        /// Gets the subsequent messages of this conversation.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly"), DataMember]
        public ICollection<DialogueMessageDto> Messages { get; private set; }

        /// <summary>
        /// Gets or sets the reciever.
        /// </summary>
        [DataMember]
        public PersonDto Receiver { get; set; }

        /// <summary>
        /// Gets or sets the original date.
        /// </summary>
        [DataMember]
        public string OriginalDate { get; set; }

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        [DataMember]
        public string Message { get; set; }
    }
}