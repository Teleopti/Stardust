using System;
using Teleopti.Interfaces.Messages;

namespace Teleopti.Ccc.Domain.ApplicationRtaQueue
{
    /// <summary>
    /// 
    /// </summary>
    public class BusinessUnitInfo : RaptorDomainMessage
    {
        private readonly Guid _messageId = Guid.NewGuid();

        ///<summary>
        /// Definies an identity for this message (typically the Id of the root this message refers to.
        ///</summary>
        public override Guid Identity
        {
            get { return _messageId; }
        }
    }
}
