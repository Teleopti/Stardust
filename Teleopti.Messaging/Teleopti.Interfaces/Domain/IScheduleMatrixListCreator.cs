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

        /// <summary>
        /// Creates the matrix list from schedule parts and alterantive schedule ranges.
        /// </summary>
        /// <param name="rangeClones">The range clones.</param>
        /// <param name="scheduleDays">The schedule days.</param>
        /// <returns></returns>
        IList<IScheduleMatrixPro> CreateMatrixListFromSchedulePartsAndAlternativeScheduleRanges(
            IDictionary<IPerson, IScheduleRange> rangeClones, IEnumerable<IScheduleDay> scheduleDays);
    }
}