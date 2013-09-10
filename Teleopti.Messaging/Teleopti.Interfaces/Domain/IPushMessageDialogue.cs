using System.Collections.Generic;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// The dialogue between two parts caused by a conversation
    /// </summary>
    public interface IPushMessageDialogue : IAggregateRoot, 
                                            IMainReference, 
                                            IReplyOptionTarget<string>,
                                            IChangeInfo,
																						ICreateInfo
    {
        /// <summary>
        /// Gets the conversation.
        /// </summary>
        /// <value>The conversation.</value>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2009-05-19
        /// </remarks>
        IPushMessage PushMessage { get; }

        /// <summary>
        /// Gets the receiver.
        /// </summary>
        /// <value>The receiver.</value>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2009-05-19
        /// </remarks>
        IPerson Receiver { get; }

        /// <summary>
        /// Gets the messages.
        /// </summary>
        /// <value>The messages.</value>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2009-05-19
        /// </remarks>
        IList<IDialogueMessage> DialogueMessages { get; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is replied.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is replied; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2009-05-26
        /// </remarks>
        bool IsReplied { get; }

	    /// <summary>
	    /// Gets the message.
	    /// </summary>
	    /// <value>The message.</value>
	    /// <remarks>
	    /// Created by: henrika
	    /// Created date: 2009-11-02
	    /// </remarks>
	    string Message(ITextFormatter textFormatter);

        /// <summary>
        /// Replies to the PushMessage. Text replies are only allowed when AllowDialogueReply is true for the PushMessage this dialogue belongs to.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="sender">The sender.</param>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2009-05-19
        /// </remarks>
        void DialogueReply(string message, IPerson sender);

        /// <summary>
        /// Clears the reply and sets the IsReplied to false
        /// </summary>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2009-06-05
        /// </remarks>
        void ClearReply();
    }
}