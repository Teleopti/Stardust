namespace Teleopti.Ccc.Domain.Common.Messaging
{
    /// <summary>
    /// Sends a PushMessage on persist if changed
    /// </summary>
    /// <remarks>
    /// Created by: henrika
    /// Created date: 2009-10-21
    /// </remarks>
    public interface IPushMessageWhenRootAltered
    {
        /// <summary>
        /// Checks if PushMessage should be sent
        /// </summary>
        /// <returns>
        /// true if a pushmessage should be created 
        /// </returns>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2009-10-21
        /// </remarks>
        bool ShouldSendPushMessageWhenAltered();

        /// <summary>
        /// PushMessageInformation (Pushmessage, receivers, replyoptions, etc)
        /// </summary>
        /// <returns>
        /// ISendPushMessageService containing the information (Pushmessage, receivers, replyoptions, etc)
        /// </returns>
        ISendPushMessageService PushMessageWhenAlteredInformation();
    }
}
