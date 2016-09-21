using System;
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

        ISkill LoadSkill(ISkill skill);
	    IEnumerable<ISkill> LoadAllSkills();
		IMultisiteSkill LoadMultisiteSkill(ISkill skill);
	    int MinimumResolution();
	    IEnumerable<ISkill> FindSkillsWithAtLeastOneQueueSource();
	    ICollection<ISkill> LoadSkills(IEnumerable<Guid> skillIdList);

    }
}