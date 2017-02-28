using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
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