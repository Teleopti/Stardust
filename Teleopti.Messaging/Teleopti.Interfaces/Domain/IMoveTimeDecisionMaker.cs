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
        /// <returns></returns>
        IList<DateOnly> Execute(IScheduleMatrixPro matrix, IScheduleResultDataExtractor dataExtractor);
    }
}