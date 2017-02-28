using System;

namespace Teleopti.Interfaces.Messages.Requests
{
    public class AcceptShiftTrade : MessageWithLogOnContext
    {
        public override Guid Identity
        {
            get { return PersonRequestId; }
        }

        public Guid AcceptingPersonId { get; set; }

        public Guid PersonRequestId { get; set; }

        public string Message { get; set; }
    }
}
