namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Payload that hold Meeting stuff.
    /// </summary>
    /// <remarks>
    /// Created by: henryg
    /// Created date: 2009-10-22
    /// </remarks>
    public interface IMeetingPayload : IPayload
    {
        /// <summary>
        /// Gets the meeting.
        /// </summary>
        /// <value>The meeting.</value>
        /// <remarks>
        /// Created by: henryg
        /// Created date: 2009-10-22
        /// </remarks>
        IMeeting Meeting { get; }
    }
}