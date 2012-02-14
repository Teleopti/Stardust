namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Converts a ScheduleMatrix to a LockableBitArray
    /// </summary>
    public interface IScheduleMatrixLockableBitArrayConverter
    {
        /// <summary>
        /// Converts the matrix.
        /// </summary>
        /// <param name="useWeekBefore">if set to <c>true</c> [use week before].</param>
        /// <param name="useWeekAfter">if set to <c>true</c> [use week after].</param>
        /// <returns></returns>
        ILockableBitArray Convert(bool useWeekBefore, bool useWeekAfter);

        /// <summary>
        /// Gets the source matrix.
        /// </summary>
        /// <value>The source matrix.</value>
        IScheduleMatrixPro SourceMatrix { get; }

        /// <summary>
        /// Counts the workdays in the source matrix.
        /// </summary>
        /// <returns></returns>
        int Workdays();

        /// <summary>
        /// Counts the day offs in the source matrix.
        /// </summary>
        /// <returns></returns>
        int DayOffs();
    }
}