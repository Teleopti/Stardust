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


        /// <summary>
        /// Finds all assignments for a person where there is a certain absence 
        /// </summary>
        /// <param name="period">The period.</param>
        /// <param name="scenario">The scenario.</param>
        /// <param name="person">The agent.</param>
        /// <param name="absence">The search.</param>
        /// <returns>
        /// All underlying asignments 
        /// </returns>
        /// <remarks>
        /// calculates the periods if there is any absence on that period, even if the highest in projection
        /// </remarks>
        IEnumerable<IPersonAssignment> Find(DateTimePeriod period, IScenario scenario, IPerson person, IAbsence absence);
    }
}