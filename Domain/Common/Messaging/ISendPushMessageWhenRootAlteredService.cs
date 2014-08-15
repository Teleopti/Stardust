using System.Collections.Generic;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.Common.Messaging
{
    /// <summary>
    /// Service for generating pushmessages when items are changed
    /// </summary>
    public interface ISendPushMessageWhenRootAlteredService
    {
        /// <summary>
        /// Sends the push messages to all changed roots that implements IPushMessageWhenRootAltered
        /// </summary>
        /// <param name="changedRoots">The changed roots.</param>
        /// <param name="repository">The repository.</param>
        /// <returns>
        /// >The roots that has been created by sending the message (PushMessage,PushMessageDialogue)
        /// </returns>
        /// <remarks>
        /// Checks if the items ShouldSend
        /// Created by: henrika
        /// Created date: 2009-10-22
        /// </remarks>
		IList<IAggregateRoot> SendPushMessages(IEnumerable<IRootChangeInfo> changedRoots, IPushMessagePersister repository);

     }
}
