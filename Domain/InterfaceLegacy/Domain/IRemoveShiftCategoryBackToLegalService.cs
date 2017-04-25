using System.Collections.Generic;
using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
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
		/// <param name="schedulingOptions">The scheduling options.</param>
		/// <returns></returns>
        IList<IScheduleDayPro> Execute(IShiftCategoryLimitation shiftCategoryLimitation, SchedulingOptions schedulingOptions);

        /// <summary>
        /// Schedule matrix for a single person
        /// </summary>
        IScheduleMatrixPro ScheduleMatrixPro { get; }
    }
}