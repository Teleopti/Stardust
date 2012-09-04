using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Decides and perform bitarray move(s) on intraday
    /// </summary>
    public interface IIntradayDecisionMaker
    {
		/// <summary>
		/// Executes the specified lockable bit array.
		/// </summary>
		/// <param name="lockableBitArray">The lockable bit array.</param>
		/// <param name="dataExtractor">The data extractor.</param>
		/// <param name="matrix">The matrix.</param>
		/// <returns></returns>
		DateOnly? Execute(ILockableBitArray lockableBitArray, IScheduleResultDataExtractor dataExtractor, IScheduleMatrixPro matrix);

        /// <summary>
        /// Excecutes the specified lockable bit array.
        /// </summary>
        /// <param name="matrixConverter">The matrix converter.</param>
        /// <param name="dataExtractor">The data extractor.</param>
        /// <returns></returns>
        DateOnly? Execute(IScheduleMatrixLockableBitArrayConverter matrixConverter, IScheduleResultDataExtractor dataExtractor);
    }
}