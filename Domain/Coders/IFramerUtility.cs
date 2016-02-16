using System.IO;

namespace Teleopti.Ccc.Domain.Coders
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