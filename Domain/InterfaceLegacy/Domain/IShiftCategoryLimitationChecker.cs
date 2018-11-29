using System.Collections.Generic;
using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    /// <summary>
    /// Interface for ShiftCategoryLimitationChecker
    /// </summary>
    public interface IShiftCategoryLimitationChecker
    {
    	/// <summary>
    	/// Sets the blocked shift categories.
    	/// </summary>
    	/// <param name="optimizerPreferences">The optimizer preferences.</param>
    	/// <param name="person"></param>
    	/// <param name="dateOnly"></param>
        void SetBlockedShiftCategories(SchedulingOptions optimizerPreferences, IPerson person, DateOnly dateOnly);

        /// <summary>
        /// Determines whether [is shift category over period limit] [the specified shift category limitation].
        /// </summary>
        /// <param name="shiftCategoryLimitation">The shift category limitation.</param>
        /// <param name="schedulePeriodDates">The schedule period dates.</param>
        /// <param name="personRange">The person range.</param>
        /// <param name="datesWithCategory">Return a list of dates with shift of the category.</param>
        /// <returns>
        /// 	<c>true</c> if [is shift category over period limit] [the specified shift category limitation]; otherwise, <c>false</c>.
        /// </returns>
       bool IsShiftCategoryOverPeriodLimit(IShiftCategoryLimitation shiftCategoryLimitation,
                                                DateOnlyPeriod schedulePeriodDates, IScheduleRange personRange, out IList<DateOnly> datesWithCategory);


       /// <summary>
       /// Determines whether [is shift category over week limit] [the specified shift category limitation].
       /// </summary>
       /// <param name="shiftCategoryLimitation">The shift category limitation.</param>
       /// <param name="personRange">The person range.</param>
       /// <param name="queryWeek">The Week.</param>
       /// <param name="datesWithCategory">Return a list of dates with shift of the category.</param>
       /// <returns>
       /// 	<c>true</c> if [is shift category over week limit] [the specified shift category limitation]; otherwise, <c>false</c>.
       /// </returns>
        bool IsShiftCategoryOverWeekLimit(IShiftCategoryLimitation shiftCategoryLimitation,
                                              IScheduleRange personRange, DateOnlyPeriod queryWeek, out IList<DateOnly> datesWithCategory);
    }
}