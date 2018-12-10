using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    /// <summary>
    /// 
    /// </summary>
    /// /// 
    /// <remarks>
    ///  Created by: Ola
    ///  Created date: 2008-11-17    
    /// /// </remarks>
    public interface ISkillStaffPeriodHolder
    {
	    IDictionary<IActivity, IDictionary<DateTime, ISkillStaffPeriodDataHolder>> SkillStaffDataPerActivity(
		    DateTimePeriod onPeriod, IList<ISkill> onSkills, ISkillPriorityProvider skillPriorityProvider);
        
		/// <summary>
        /// Gets the skill skill staff period dictionary.
        /// </summary>
        /// <value>The skill skill staff period dictionary.</value>
        /// /// 
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-11-17    
        /// /// </remarks>
        ISkillSkillStaffPeriodExtendedDictionary SkillSkillStaffPeriodDictionary { get; }

        /// <summary>
        /// Creates a skill staff period list with all periods for given skills and period.
        /// </summary>
        /// <param name="skills">The skills.</param>
        /// <param name="utcPeriod">The period in Utc.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-12-09
        /// </remarks>
        IList<ISkillStaffPeriod> SkillStaffPeriodList(IEnumerable<ISkill> skills, DateTimePeriod utcPeriod);

        /// <summary>
        /// Creates a skill staff utcPeriod list with all periods for given virtual skill's skills and utcPeriod.
        /// </summary>
        /// <param name="skill">The skill.</param>
        /// <param name="utcPeriod">The period in Utc.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2009-01-19
        /// </remarks>
        IList<ISkillStaffPeriod> SkillStaffPeriodList(IAggregateSkill skill, DateTimePeriod utcPeriod);

        /// <summary>
        /// Creates a skill staff period dictionary with all periods for given virtual skill's skills and period.
        /// </summary>
        /// <param name="skill">The skill.</param>
        /// <param name="utcPeriod">The period in Utc.</param>
        /// <param name="forDay">if set to <c>true</c> [for day].</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2009-01-22
        /// </remarks>
        ISkillStaffPeriodDictionary SkillStaffPeriodList(IAggregateSkill skill, DateTimePeriod utcPeriod,
                                                                            bool forDay);
		/// <summary>
		/// Creates a skill staff period list with all periods that intersects given period and on given skills.
		/// </summary>
		/// <param name="skills">The skills.</param>
		/// <param name="utcPeriod">The UTC period.</param>
		/// <returns></returns>
		IList<ISkillStaffPeriod> IntersectingSkillStaffPeriodList(IEnumerable<ISkill> skills, DateTimePeriod utcPeriod);

		/// <summary>
		/// Skills the staff period dictionary.
		/// </summary>
		/// <param name="skills">The skills.</param>
		/// <param name="utcPeriod">The UTC period.</param>
		/// <returns></returns>
		IDictionary<ISkill, ISkillStaffPeriodDictionary> SkillStaffPeriodDictionary(IEnumerable<ISkill> skills,
																					DateTimePeriod utcPeriod);
    }
}
