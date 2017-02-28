using System;

namespace Teleopti.Interfaces.Messages.Requests
{
    public class NewShiftTradeRequestCreated : MessageWithLogOnContext
    {
        public override Guid Identity
        {
            get
            {
                return PersonRequestId;
            }
        }

        public Guid PersonRequestId { get; set; }
    }
}
