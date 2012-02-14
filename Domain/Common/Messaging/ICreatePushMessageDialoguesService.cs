using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common.Messaging
{
    /// <summary>
    /// For creating PushMessageDialogues for each receiver and generate a receipt
    /// </summary>
    /// <remarks>
    /// Created by: henrika
    /// Created date: 2009-10-22
    /// </remarks>
    public interface ICreatePushMessageDialoguesService
    {
        /// <summary>
        /// Creates the specified pushmessage graph by adding a PushMessageDialogue for every receiver.
        /// </summary>
        /// <param name="pushMessage">The push message.</param>
        /// <param name="receivers">The receivers.</param>
        /// <returns>
        /// A receipt with the created entities
        /// </returns>
        /// <remarks>
        /// The receipt is used for notifying the messagebroker. Because we need to add the new items to the list of changed roots
        /// Created by: henrika
        /// Created date: 2009-10-22
        /// </remarks>
        ISendPushMessageReceipt Create(IPushMessage pushMessage, IEnumerable<IPerson> receivers);
    }
}