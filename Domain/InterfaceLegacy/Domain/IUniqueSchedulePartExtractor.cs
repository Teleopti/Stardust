using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    /// <summary>
    /// Extracts the unique schedule parts.
    /// </summary>
    public interface IUniqueSchedulePartExtractor
    {
        /// <summary>
        /// Extracts the unique schedule parts from the schedule parts list in the form of a <see cref="ISchedulePartExtractor"/>.
        /// </summary>
        /// <param name="schedulePartList">The schedule part list.</param>
        /// <returns></returns>
        IEnumerable<ISchedulePartExtractor> ExtractUniqueScheduleParts(IEnumerable<IScheduleDay> schedulePartList);
    }
}