using System;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// DialogueReply for a ConversationDialogue
    /// </summary>
    /// <remarks>
    /// Created by: henrika
    /// Created date: 2009-05-19
    /// </remarks>
    public interface IDialogueMessage
    {
        /// <summary>
        /// Gets the text.
        /// </summary>
        /// <value>The text.</value>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2009-05-19
        /// </remarks>
        string Text { get; }

        /// <summary>
        /// Gets when the message is created.
        /// </summary>
        /// <value>The created.</value>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2009-05-26
        /// </remarks>
        DateTime Created { get;}

        /// <summary>
        /// Gets the sender of the message
        /// </summary>
        /// <value>The sender.</value>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2009-05-26
        /// </remarks>
        IPerson Sender { get; }

    }
}