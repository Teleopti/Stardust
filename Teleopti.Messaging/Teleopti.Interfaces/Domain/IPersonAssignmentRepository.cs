using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    ///  Interface for AgentAssignmentRepository
    /// </summary>
    public interface IPersonAssignmentRepository : IRepository<IPersonAssignment>, ILoadAggregateById<IPersonAssignment>
    {
        /// <summary>
        /// Finds the specified persons.
        /// </summary>
        /// <param name="persons">The persons.</param>
        /// <param name="period">The period.</param>
        /// <param name="scenario">The scenario.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: Sumedah
        /// Created date: 2008-03-06
        /// </remarks>
        ICollection<IPersonAssignment> Find(IEnumerable<IPerson> persons,
                                            DateTimePeriod period,
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
        ICollection<IPersonAssignment> Find(DateTimePeriod period, IScenario scenario);

        /// <summary>
        /// Removes the main shift from the data source.
        /// </summary>
        /// <param name="personAssignment">The agent assignment.</param>
        void RemoveMainShift(IPersonAssignment personAssignment);
    }
}