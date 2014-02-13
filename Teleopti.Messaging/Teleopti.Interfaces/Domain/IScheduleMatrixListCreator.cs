using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// <see cref="IScheduleMatrixPro"/> list creator class.
    /// </summary>
    public interface IScheduleMatrixListCreator
    {
        /// <summary>
        /// Creates a <see cref="IScheduleMatrixPro"/> list from schedule parts.
        /// </summary>
        /// <param name="scheduleDays">The schedule parts.</param>
        /// <returns></returns>
        IList<IScheduleMatrixPro> CreateMatrixListFromScheduleParts(IEnumerable<IScheduleDay> scheduleDays);
    }
}