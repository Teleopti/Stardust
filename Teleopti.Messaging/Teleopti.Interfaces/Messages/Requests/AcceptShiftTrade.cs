using System;

namespace Teleopti.Interfaces.Messages.Requests
{
    ///<summary>
    ///Message used for accepting a shift trade
    ///</summary>
    public class AcceptShiftTrade : MessageWithLogOnContext
    {
        /// <summary>
        /// Identity for this message
        /// </summary>
        public override Guid Identity
        {
            get { return PersonRequestId; }
        }

        ///<summary>
        ///The id of the person accepting
        ///</summary>
        public Guid AcceptingPersonId { get; set; }

        ///<summary>
        ///Person request id for the shift trade
        ///</summary>
        public Guid PersonRequestId { get; set; }

        ///<summary>
        /// The message to try to set to the person request while accepting.
        ///</summary>
        public string Message { get; set; }
    }
}
