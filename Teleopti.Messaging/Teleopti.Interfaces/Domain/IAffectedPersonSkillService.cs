using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Service to calculate the <see cref="IPersonSkill"/> that are affected based on person, activity and period.
    /// </summary>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 2009-02-25
    /// </remarks>
    public interface IAffectedPersonSkillService
    {
        /// <summary>
        /// Executes the calculation.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="activity">The activity.</param>
        /// <param name="dateOnly">The date only.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2009-02-25
        /// </remarks>
        ICollection<IPersonSkill> Execute(IPerson person, IActivity activity, DateOnly dateOnly);

        /// <summary>
        /// Gets the valid skill collection.
        /// </summary>
        /// <value>The valid skill collection.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2009-02-25
        /// </remarks>
        IEnumerable<ISkill> AffectedSkills { get; }
    }


}
