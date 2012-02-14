using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]
    public interface IRtaStateGroupRepository : IRepository<IRtaStateGroup>
    {
        /// <summary>
        /// Loads all complete graph.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-11-19
        /// </remarks>
        IList<IRtaStateGroup> LoadAllCompleteGraph();
    }
}