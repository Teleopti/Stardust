using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{

    /// <summary>
    /// Provider of replyoptions
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <remarks>
    /// Created by: henrika
    /// Created date: 2009-05-25
    /// </remarks>
    public interface IReplyOptionsSource<T>
    {

        /// <summary>
        /// Gets the reply options.
        /// </summary>
        /// <value>The reply options.</value>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2009-05-25
        /// </remarks>
        IList<T> ReplyOptions { get;}

        /// <summary>
        /// Checks if the reply is valid.
        /// </summary>
        /// <param name="replyToCheck">The reply to check.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2009-05-25
        /// </remarks>
        bool CheckReply(T replyToCheck);


    }
}