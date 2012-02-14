using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common.Messaging
{
    /// <summary>
    /// Information about the sent PushMessageDialogues and the PushMessage
    /// </summary>
    /// <remarks>
    /// Created by: henrika
    /// Created date: 2009-10-22
    /// </remarks>
    public interface ISendPushMessageReceipt
    {
        /// <summary>
        /// Gets the  roots generated by the message.
        /// </summary>
        /// <value>The changed roots.</value>
        /// <remarks>
        /// PushMessage / PushMessageDialogues
        /// Created by: henrika
        /// Created date: 2009-10-22
        /// </remarks>
        IList<IAggregateRoot> AddedRoots();

        /// <summary>
        /// Gets the created push message
        /// </summary>
        /// <value>The created PushMessage</value>
        /// <remarks>
        /// Always One
        /// Created by: henrika
        /// Created date: 2009-10-22
        /// </remarks>
        IPushMessage CreatedPushMessage { get; }

        /// <summary>
        /// Gets the created dialogues.
        /// </summary>
        /// <value>The created dialogues.</value>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2009-10-22
        /// </remarks>
        IList<IPushMessageDialogue> CreatedDialogues { get; }

    }
}