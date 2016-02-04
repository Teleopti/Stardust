using System;

namespace Teleopti.Interfaces.Messages.Requests
{
    /// <summary>
    /// Message to inform consumers that a new shift trade request has been created
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2010-02-24
    /// </remarks>
    public class NewShiftTradeRequestCreated : MessageWithLogOnContext
    {
        /// <summary>
        /// Identity for this message
        /// </summary>
        public override Guid Identity
        {
            get
            {
                return PersonRequestId;
            }
        }

        /// <summary>
        /// Gets or sets the person request id.
        /// </summary>
        /// <value>The person request id.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2010-02-24
        /// </remarks>
        public Guid PersonRequestId { get; set; }
    }
}
