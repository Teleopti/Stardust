using System.IO;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Interfaces.MessageBroker.Coders
{
    /// <summary>
    /// Decodes an Event Message Stream.
    /// </summary>
    public interface IEventMessageDecoder
    {
        /// <summary>
        /// Decodes an Event Message stream.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        IEventMessage Decode(Stream source);
        /// <summary>
        /// Decodes an Event Message packet.
        /// </summary>
        /// <param name="packet"></param>
        /// <returns></returns>
        IEventMessage Decode(byte[] packet);
    }
}