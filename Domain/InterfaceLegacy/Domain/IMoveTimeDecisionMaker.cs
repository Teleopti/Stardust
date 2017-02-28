using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
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