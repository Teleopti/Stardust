namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Basic communication for internal messaging
    /// </summary>
    public interface IPushMessage : IAggregateRoot, 
                                    IReplyOptionsSource<string>,
                                    IChangeInfo,
																		ICreateInfo
    {
        /// <summary>
        /// Gets or sets the sender of the conversation.
        /// </summary>
        /// <value>The sender.</value>
        IPerson Sender { get; set; }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        /// <value>The title.</value>
        string Title { set; }

    	/// <summary>
    	/// Get the formatted title
    	/// </summary>
    	/// <param name="formatter"></param>
    	/// <returns></returns>
    	string GetTitle(ITextFormatter formatter);

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        /// <value>The message.</value>
        string Message { set; }

    	/// <summary>
    	/// Get the formatted message
    	/// </summary>
    	/// <param name="formatter"></param>
    	/// <returns></returns>
    	string GetMessage(ITextFormatter formatter);

        /// <summary>
        /// Gets or sets a value indicating whether to allow dialogue replies.
        /// </summary>
        /// <value><c>true</c> if allowing dialogue replies; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2009-05-29
        /// </remarks>
        bool AllowDialogueReply { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the context of the pushmessage is intended to be translated.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [translate message]; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2009-11-02
        /// </remarks>
        bool TranslateMessage{ get; set; }

		/// <summary>
		/// Get or set a value indicating the type of the push message
		/// </summary>
		/// <remarks>
		/// Created by: Xinfeng Li
		/// Created date: 2014-08-18
		/// </remarks>
		MessageType MessageType { get; set; }
    }
}