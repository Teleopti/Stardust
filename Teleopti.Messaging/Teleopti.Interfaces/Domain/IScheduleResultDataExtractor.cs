using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Different types of data from skillstaff periods or skill days
    /// </summary>
    public interface IScheduleResultDataExtractor
    {
        /// <summary>
        /// A list of values.
        /// </summary>
        /// <returns></returns>
        IList<double?> Values();
    }
}