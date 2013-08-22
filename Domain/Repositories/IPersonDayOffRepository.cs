using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
    /// <summary>
    ///  Interface for IPersonDayOffRepository
    /// </summary>
    public interface IPersonDayOffRepository : IRepository<IPersonDayOff>, ILoadAggregateById<IPersonDayOff>
    {
        /// <summary>
        /// Finds the specified DayOffs.
        /// </summary>
        /// <param name="agents">The agents.</param>
        /// <returns></returns>
        ICollection<IPersonDayOff> Find(IEnumerable<IPerson> agents);

        /// <summary>
        /// Finds the specified days off.
        /// </summary>
        /// <param name="agents">The agents.</param>
        /// <param name="period">The period.</param>
        /// <returns></returns>
        ICollection<IPersonDayOff> Find(IEnumerable<IPerson> agents,
                                      DateTimePeriod period);

        /// <summary>
        /// Finds the specified agents.
        /// </summary>
        /// <param name="agents">The agents.</param>
        /// <param name="period">The period.</param>
        /// <param name="scenario">The scenario.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: Sumedah
        /// Created date: 2008-03-06
        /// </remarks>
        ICollection<IPersonDayOff> Find(IEnumerable<IPerson> agents, DateTimePeriod period,
                                       IScenario scenario);

        /// <summary>
        /// Finds the specified period.
        /// </summary>
        /// <param name="period">The period.</param>
        /// <param name="scenario">The scenario.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2007-11-12
        /// </remarks>
        ICollection<IPersonDayOff> Find(DateTimePeriod period, IScenario scenario);
    }
}