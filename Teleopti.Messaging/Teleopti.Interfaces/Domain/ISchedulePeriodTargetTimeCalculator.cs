using System;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Interface for classes to calculate the period target.
    /// </summary>
    public interface ISchedulePeriodTargetTimeCalculator
    {

        /// <summary>
        /// Target the with tolerance.
        /// </summary>
        /// <returns></returns>
        TimePeriod TargetWithTolerance(IScheduleMatrixPro matrix);

		/// <summary>
		/// Targets the time.
		/// </summary>
		/// <param name="matrix">The matrix.</param>
		/// <returns></returns>
    	TimeSpan TargetTime(IScheduleMatrixPro matrix);
    }
}