using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Teleopti.Interfaces.Messages.Denormalize
{
    public class RTAStateCheckerMessage : RaptorDomainMessage
    {
        private readonly Guid _messageId = Guid.NewGuid();
        
        public override Guid Identity
        {
            get { return _messageId; }
        }
    }
}
