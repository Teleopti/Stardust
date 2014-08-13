using System;

namespace Teleopti.Interfaces.Messages.Rta
{
    /// <summary>
    /// 
    /// </summary>
    public class StartUpBusinessUnit : RaptorDomainMessage
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
