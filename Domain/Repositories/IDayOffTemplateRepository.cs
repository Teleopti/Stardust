using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
    /// <summary>
    /// Interface for DayOffRepository
    /// </summary>
    public interface IDayOffTemplateRepository : IRepository<IDayOffTemplate>
    {
        /// <summary>
        /// Finds all day-off data and return them, sorted by the description
        /// </summary>
        /// <returns>A list of <see cref="IDayOff"/></returns>
        /// <remarks>
        /// Created by: shirang
        /// Created date: 29/10/2008
        /// </remarks>
        IList<IDayOffTemplate> FindAllDayOffsSortByDescription();
    }
}
