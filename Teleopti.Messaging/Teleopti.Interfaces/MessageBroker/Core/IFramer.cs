using System;
using System.IO;

namespace Teleopti.Interfaces.MessageBroker.Core
{
    /// <summary>
    /// Framer Utility
    /// </summary>
    /// <remarks>
    /// Created by: ankarlp
    /// Created date: 2008-08-07
    /// </remarks>
    public interface IFramerUtility
    {
        /// <summary>
        /// Reads the end.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 2008-08-07
        /// </remarks>
        byte[] ReadEnd(Stream source);
        /// <summary>
        /// Nexts the token.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="delimiter">The delimiter.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 2008-08-07
        /// </remarks>
        byte[] NextToken(Stream input, byte[] delimiter);
    }
}