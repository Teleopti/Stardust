using System.IO;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Interfaces.MessageBroker.Coders
{
    /// <summary>
    /// Decodes an Event User.
    /// </summary>
    public interface IEventUserDecoder
    {
        /// <summary>
        /// Decode an event user stream.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        IEventUser Decode(Stream source);
        /// <summary>
        /// Decode a byte array package.
        /// </summary>
        /// <param name="packet"></param>
        /// <returns></returns>
        IEventUser Decode(byte[] packet);
    }
}