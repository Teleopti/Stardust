namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Set checksum to a shift trade request
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2009-09-15
    /// </remarks>
    public interface IShiftTradeRequestSetChecksum
    {
        /// <summary>
        /// Sets the checksum.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2009-09-15
        /// </remarks>
        void SetChecksum(IRequest request);
    }
}