using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
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

		/// <summary>
		/// Times the target
		/// </summary>
		/// <param name="virtualSchedulePeriod"></param>
		/// <param name="scheduleDays"></param>
		/// <returns></returns>
		TimeSpan TargetTime(IVirtualSchedulePeriod virtualSchedulePeriod, IEnumerable<IScheduleDay> scheduleDays);

    	/// <summary>
    	/// Tolerates the targets time
    	/// </summary>
    	/// <param name="virtualSchedulePeriod"></param>
    	/// <param name="scheduleDays"></param>
    	/// <returns></returns>
		TimePeriod TargetTimeWithTolerance(IVirtualSchedulePeriod virtualSchedulePeriod, IEnumerable<IScheduleDay> scheduleDays);
    }
}