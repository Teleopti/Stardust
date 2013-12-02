using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Decides and perform bitarray move(s) on days off
    /// </summary>
    public interface IMoveTimeDecisionMaker
    {
        /// <summary>
        /// Excecutes the specified lockable bit array.
        /// </summary>
        /// <param name="matrixConverter">The matrix converter.</param>
        /// <param name="dataExtractor">The data extractor.</param>
        /// <returns></returns>
        IList<DateOnly> Execute(IScheduleMatrixLockableBitArrayConverter matrixConverter, IScheduleResultDataExtractor dataExtractor);
    }
}