using System.Collections.ObjectModel;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common.Messaging
{

    public interface ISendPushMessageService : ISendPushMessageOption<string>
    {
        /// <summary>
        /// Sends the conversation.
        /// </summary>
		void SendConversation(IPushMessagePersister repository);


        /// <summary>
        /// Sends the conversation with receipt.
        /// </summary>
        /// <param name="repository">The repository.</param>
        /// <returns>
        /// Info about created roots
        /// </returns>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2009-10-22
        /// </remarks>
		ISendPushMessageReceipt SendConversationWithReceipt(IPushMessagePersister repository);

        /// <summary>
        /// Sends the conversation.
        /// </summary>
        /// <param name="pushMessageRepository">The conversation repository.</param>
        /// <param name="pushMessageDialogueRepository">The conversation dialogue repository.</param>
        /// <remarks>
        /// The converstaionDialogueRepository is for creating and persisiting a conversationdialogue for ech receiver
        /// </remarks>
        void SendConversation(IPushMessageRepository pushMessageRepository,
                              IPushMessageDialogueRepository pushMessageDialogueRepository);

        /// <summary>
        /// Clears the receivers.
        /// </summary>
        void ClearReceivers();

        /// <summary>
        /// Gets the conversation.
        /// </summary>
        /// <value>The conversation.</value>
        IPushMessage PushMessage { get; }

        /// <summary>
        /// Gets the receivers.
        /// </summary>
        /// <value>The receivers.</value>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2009-05-27
        /// </remarks>
        ReadOnlyCollection<IPerson> Receivers { get; }
    }

   

   

}
