using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Removes the days with shift categories that are breaking the rules from the schedule period
    /// so that the schedule period gets back to legal state.
    /// </summary>
    public interface IRemoveShiftCategoryBackToLegalService
    {
        /// <summary>
        /// Executes back to legal state on the specified shift category limitation.
        /// </summary>
        /// <param name="shiftCategoryLimitation">The shift category limitation.</param>
        /// <returns></returns>
        IList<IScheduleDayPro> Execute(IShiftCategoryLimitation shiftCategoryLimitation);

    }
}