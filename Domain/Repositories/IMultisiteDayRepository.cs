using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2008-05-08
    /// </remarks>
    public interface IMultisiteDayRepository
    {
        /// <summary>
        /// Finds the range.
        /// </summary>
        /// <param name="period">The period.</param>
        /// <param name="skill">The skill.</param>
        /// <param name="scenario">The scenario.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-05-08
        /// </remarks>
        ICollection<IMultisiteDay> FindRange(DateOnlyPeriod period, ISkill skill, IScenario scenario);

        /// <summary>
        /// Gets all multisite days.
        /// </summary>
        /// <param name="period">The period.</param>
        /// <param name="multisiteDays">The multisite days.</param>
        /// <param name="skill">The skill.</param>
        /// <param name="scenario">The scenario.</param>
        /// <param name="addToRepository">if set to <c>true</c> [add to repository].</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-05-08
        /// </remarks>
        ICollection<IMultisiteDay> GetAllMultisiteDays(DateOnlyPeriod period, ICollection<IMultisiteDay> multisiteDays, IMultisiteSkill skill, IScenario scenario, bool addToRepository);

        /// <summary>
        /// Gets all multisite days.
        /// </summary>
        /// <param name="period">The period.</param>
        /// <param name="multisiteDays">The multisite days.</param>
        /// <param name="skill">The skill.</param>
        /// <param name="scenario">The scenario.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-05-08
        /// </remarks>
        ICollection<IMultisiteDay> GetAllMultisiteDays(DateOnlyPeriod period, ICollection<IMultisiteDay> multisiteDays, IMultisiteSkill skill, IScenario scenario);

        /// <summary>
        /// Deletes the specified date time period.
        /// </summary>
        /// <param name="dateTimePeriod">The date time period.</param>
        /// <param name="multisiteSkill">The multisite skill.</param>
        /// <param name="scenario">The scenario.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-12-22
        /// </remarks>
        void Delete(DateOnlyPeriod dateTimePeriod, IMultisiteSkill multisiteSkill, IScenario scenario);
    }
}
