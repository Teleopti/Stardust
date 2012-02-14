#region Imports

using System.Runtime.Serialization;

#endregion

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject
{
    /// <summary>
    /// Represents a FaultDto object.
    /// </summary>
    [DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2010/04/")]
    public class FaultDto
    {

        [DataMember(Name = "Message")]
        private string _message;

        /// <summary>
        /// Initializes a new instance of the <see cref="FaultDto"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public FaultDto(string message)
        {
            _message = message;
        }

        /// <summary>
        /// Gets or sets the fault code.
        /// </summary>
        /// <value>The fault code.</value>
        [DataMember]
        public int FaultCode { get; set; }

        /// <summary>
        /// Gets or sets the message id.
        /// </summary>
        /// <value>The message id.</value>
        [DataMember]
        public string MessageId { get; set; }

        /// <summary>
        /// Gets the message.
        /// </summary>
        /// <value>The message.</value>
        public string Message
        {
            get
            {
                return _message;
            }
        }

        /// <summary>
        /// Gets or sets the source.
        /// </summary>
        /// <value>The source.</value>
        [DataMember]
        public string Source { get; set; }
    }
}