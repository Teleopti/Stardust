using System;
using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject
{
    /// <summary>
    /// Represents a PayrollFormatDto object.
    /// </summary>
    [Serializable]
    [DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2010/04/")]
    public class PayrollFormatDto
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PayrollFormatDto"/> class.
        /// </summary>
        /// <param name="formatId">The format id.</param>
        /// <param name="name">The name.</param>
        public PayrollFormatDto(Guid formatId, string name)
        {
            FormatId = formatId;
            Name = name;
        }

        /// <summary>
        /// Gets or sets the format id.
        /// </summary>
        /// <value>The format id.</value>
        [DataMember]
        public Guid FormatId { get; private set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        [DataMember]
        public string Name { get; private set; }
    }
}