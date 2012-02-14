using System;
using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands
{
    /// <summary>
    /// This command will try to approve a request with a request id <see cref="PersonRequestId"/>. Note that, this command performs forcefully in terms of ignoring all violated business rules.
    /// </summary>
    [DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2011/07/")]
    public class ApproveRequestCommandDto : CommandDto
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
