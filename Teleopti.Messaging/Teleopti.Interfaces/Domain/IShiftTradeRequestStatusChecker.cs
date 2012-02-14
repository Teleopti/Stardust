using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Used when looking at the shift trade request status to check whether the schedule data has been changed since creation
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2009-09-04
    /// </remarks>
    public interface IShiftTradeRequestStatusChecker
    {
        /// <summary>
        /// Checks the specified shift trade request.
        /// </summary>
        /// <param name="shiftTradeRequest">The shift trade request.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2009-09-04
        /// </remarks>
        void Check(IShiftTradeRequest shiftTradeRequest);
    }
}