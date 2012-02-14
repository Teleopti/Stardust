using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common.Messaging
{
    public interface ISendPushMessageOption<T>
    {
        /// <summary>
        /// Adds a reciever to the PushMessage
        /// </summary>
        /// <param name="receiver">The receiver.</param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "To")]
        ISendPushMessageService To(IPerson receiver);

        /// <summary>
        /// Adds a reciever to the PushMessage
        /// </summary>
        /// <param name="receivers">The receivers.</param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "To")]
        ISendPushMessageService To(IList<IPerson> receivers);

        /// <summary>
        /// Sets the sender of a PushMessage.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <returns></returns>
        ISendPushMessageService From(IPerson sender);

        /// <summary>
        /// Adds a option.
        /// </summary>
        /// <param name="replyOption">The reply option.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2009-05-26
        /// </remarks>
        ISendPushMessageService AddReplyOption(T replyOption);

        /// <summary>
        /// Adds a list of options.
        /// </summary>
        /// <param name="replyOptions">The reply options.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2009-05-26
        /// </remarks>
        ISendPushMessageService AddReplyOption(IEnumerable<T> replyOptions);

        /// <summary>
        /// Translates the message to all receivers.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2009-11-02
        /// </remarks>
        ISendPushMessageService TranslateMessage();

    }

    
}
