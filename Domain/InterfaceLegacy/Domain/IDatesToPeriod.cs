using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    /// <summary>
    /// Converts a list of dates to a list of datetimeperiods
    /// </summary>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 2008-09-30
    /// </remarks>
    public interface IDatesToPeriod
    {
        /// <summary>
        /// Converts the specified date collection.
        /// </summary>
        /// <param name="dateCollection">The date collection.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-09-30
        /// </remarks>
        ICollection<DateOnlyPeriod> Convert(IEnumerable<DateOnly> dateCollection);
    }
}
