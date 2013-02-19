using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Teleopti.Interfaces.Messages.Denormalize
{
    public class RTAPersonInfoMessage: RaptorDomainMessage
    {
        private readonly Guid _messageId = Guid.NewGuid();
        private Guid _personId;
        private DateTime _activityStartDateTime;
        private DateTime _activityEndDateTime;

        public override Guid Identity
        {
            get { return _messageId; }
        }

        public Guid PersonId
        {
            get { return _personId; }
            set { _personId = value; }
        }
    }
}
