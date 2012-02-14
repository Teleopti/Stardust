using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
    public interface IStateGroupActivityAlarmRepository : IRepository<IStateGroupActivityAlarm  >
    {
        /// <summary>
        /// Loads all complete graph.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-11-21
        /// </remarks>
        IList<IStateGroupActivityAlarm> LoadAllCompleteGraph();
    }
}