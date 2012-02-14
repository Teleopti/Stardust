using System;
using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands
{
    /// <summary>
    /// This command forces a denial action on a request with <see cref="PersonRequestId"/>.
    /// </summary>
    [DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2011/07/")]
    public class DenyRequestCommandDto : CommandDto
    { 
        /// <summary>
        /// Gets and sets the request id.
        /// </summary>
        /// <value>PersonRequestId</value>
        [DataMember]
        public Guid PersonRequestId
        {
            get;
            set;
        }
    }
}
