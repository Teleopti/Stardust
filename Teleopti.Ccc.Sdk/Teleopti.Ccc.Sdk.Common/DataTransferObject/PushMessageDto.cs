using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject
{
    /// <summary>
    /// Represents a PushMessageDto object.
    /// </summary>
    [DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2010/04/")]
    public class PushMessageDto
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PushMessageDto"/> class.
        /// </summary>
        public PushMessageDto()
        {
            ReplyOptions = new List<string>();
        }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        /// <value>The title.</value>
        [DataMember]
        public string Title
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        /// <value>The message.</value>
        [DataMember]
        public string Message
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets the sender.
        /// </summary>
        /// <value>The sender.</value>
        [DataMember]
        public PersonDto Sender
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets the reply options.
        /// </summary>
        /// <value>The reply options.</value>
        [DataMember]
        public ICollection<string> ReplyOptions
        {
            get; private set;
        }

        /// <summary>
        /// Gets or sets an indication if a reply is allowed to this message.
        /// </summary>
        [DataMember]
        public bool AllowDialogueReply
        {
            get; set;
        }
    }
}