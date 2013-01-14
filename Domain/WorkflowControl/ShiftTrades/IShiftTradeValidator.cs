using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.WorkflowControl.ShiftTrades
{
    /// <summary>
    /// Class for handling all validators regarding ShiftTrade
    /// </summary>
    /// <remarks>
    /// Created by: henrika
    /// Created date: 2010-05-25
    /// Change to specification,...
    /// </remarks>
    public interface IShiftTradeValidator
    {
        /// <summary>
        /// Validates the ShiftTradeRequest
        /// </summary>
        /// <param name="shiftTradeRequest">The shift trade request.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2010-05-25
        /// </remarks>
        ShiftTradeRequestValidationResult Validate(IShiftTradeRequest shiftTradeRequest);
    }
}
