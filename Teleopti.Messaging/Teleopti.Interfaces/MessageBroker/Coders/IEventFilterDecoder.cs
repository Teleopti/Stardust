using System.IO;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Interfaces.MessageBroker.Coders
{
    /// <summary>
    /// Event filter decoder.
    /// </summary>
    public interface IEventFilterDecoder
    {
        /// <summary>
        /// Decodes a stream into an Event Filter.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        IEventFilter Decode(Stream source);
        /// <summary>
        /// Decodes a byte array package into an Event Filter.
        /// </summary>
        /// <param name="packet"></param>
        /// <returns></returns>
        IEventFilter Decode(byte[] packet);
    }
}