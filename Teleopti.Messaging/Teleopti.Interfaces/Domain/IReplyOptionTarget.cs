namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Target for ReplyOption.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <remarks>
    /// Created by: henrika
    /// Created date: 2009-05-25
    /// </remarks>
    public interface IReplyOptionTarget<T> 
    {
        /// <summary>
        /// Gets the reply.L
        /// </summary>
        /// <value>The reply.</value>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2009-05-25
        /// </remarks>
        T Reply { get; }


        /// <summary>
        /// Sets the reply.
        /// </summary>
        /// <param name="reply">The reply.</param>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2009-05-25
        /// </remarks>
        void SetReply(T reply);
    }
}