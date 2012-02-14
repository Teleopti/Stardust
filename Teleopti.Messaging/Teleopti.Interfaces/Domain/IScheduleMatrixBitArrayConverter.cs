using System.Collections;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Converts inner lists of IScheduleMatrix to a BitArray
    /// </summary>
    public interface IScheduleMatrixBitArrayConverter
    {
        /// <summary>
        /// Creates a <see cref="BitArray"/> of the outer week period days with the day offs as <c>True</c>, work days as <c>False</c>.
        /// </summary>
        /// <returns></returns>
        BitArray OuterWeekPeriodDayOffsBitArray(IScheduleMatrixPro matrix);

        /// <summary>
        /// Creates a <see cref="BitArray"/> of the outer week period days with the locked days as <c>True</c>, work days as <c>False</c>.
        /// </summary>
        /// <returns></returns>
        BitArray OuterWeekPeriodLockedDaysBitArray(IScheduleMatrixPro matrix);

        /// <summary>
        /// Gets the the index of the first and the last day of the inner period.
        /// </summary>
        /// <returns></returns>
        MinMax<int> PeriodIndexRange(IScheduleMatrixPro matrix);

    }
}