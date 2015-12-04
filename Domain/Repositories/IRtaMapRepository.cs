using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
    public interface IRtaMapRepository : IRepository<IRtaMap  >
    {
        /// <summary>
        /// Loads all complete graph.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-11-21
        /// </remarks>
        IList<IRtaMap> LoadAllCompleteGraph();
    }
}