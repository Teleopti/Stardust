using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
    /// <summary>
    /// SkillRepository Interface
    /// </summary>
    /// <remarks>
    /// Created by: zoet
    /// Created date: 2008-01-10
    /// </remarks>
    public interface ISkillRepository : IRepository<ISkill>
    {
        /// <summary>
        /// Finds all and include workload and queues.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2007-12-07
        /// </remarks>
        ICollection<ISkill> FindAllWithWorkloadAndQueues();

        /// <summary>
        /// Finds all without multisite skills.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-05-07
        /// </remarks>
        ICollection<ISkill> FindAllWithoutMultisiteSkills();

        ICollection<ISkill> FindAllWithSkillDays(DateOnlyPeriod periodWithSkillDays);


        /// <summary>
        /// Finds all with activities.
        /// </summary>
        /// <param name="activities">The activities.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: HenryG
        /// Created date: 2010-12-28
        /// </remarks>
        ICollection<ISkill> FindAllWithActivities(IEnumerable<IActivity> activities);

        ISkill LoadSkill(ISkill skill);
        IMultisiteSkill LoadMultisiteSkill(ISkill skill);
	    int MinimumResolution();
    }
}