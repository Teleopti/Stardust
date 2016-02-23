using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Decides and perform bitarray move(s) on intraday
    /// </summary>
    public interface IIntradayDecisionMaker
    {
        /// <summary>
        /// Excecutes the specified lockable bit array.
        /// </summary>
        /// <returns></returns>
		DateOnly? Execute(IScheduleMatrixPro matrix, IScheduleResultDataExtractor dataExtractor, IEnumerable<DateOnly> skipDates);
    }
}