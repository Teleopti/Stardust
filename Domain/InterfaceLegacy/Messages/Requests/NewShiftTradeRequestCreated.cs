using System;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Messages.Requests
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
