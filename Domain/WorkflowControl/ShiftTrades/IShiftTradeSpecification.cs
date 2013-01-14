using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.WorkflowControl.ShiftTrades
{
    /// <summary>
    /// Logic for checking if a part shifttrade is ok
    /// </summary>
    /// <remarks>
    /// Created by: henrika
    /// Created date: 2010-05-27
    /// </remarks>
    public interface IShiftTradeSpecification :ISpecification<IEnumerable<IShiftTradeSwapDetail>>
    {
      
        /// <summary>
        /// Verifies the specified obj.
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2010-05-27
        /// </remarks>
        ShiftTradeRequestValidationResult Validate(IEnumerable<IShiftTradeSwapDetail> obj);

        /// <summary>
        /// Gets the DenyReason (the explanation why/if the shifttrade wasnt alowed).
        /// </summary>
        /// <value>The deny reason.</value>
        /// <remarks>
        /// This returns the key, not the translated string
        /// Created by: henrika
        /// Created date: 2010-05-27
        /// </remarks>
        string DenyReason { get; }
    }
}