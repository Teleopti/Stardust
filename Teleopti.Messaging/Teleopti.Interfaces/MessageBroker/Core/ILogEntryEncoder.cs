namespace Teleopti.Interfaces.MessageBroker.Core
{
    /// <summary>
    /// The event log entry encoder.
    /// </summary>
    /// <remarks>
    /// Created by: ankarlp
    /// Created date: 2008-08-07
    /// </remarks>
    public interface IEventLogEntryEncoder
    {
        /// <summary>
        /// Encode method to turn an IEventLogEntry into a byte array.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        byte[] Encode(ILogEntry item);
    }
}