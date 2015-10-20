using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// The resource optimization helper.
    /// </summary>
    /// <remarks>
    /// Created by: peterwe
    /// Created date: 2009-01-21
    /// </remarks>
    public interface IResourceOptimizationHelper
    {
        /// <summary>
        /// Resource calculates the given date.
        /// </summary>
        /// <param name="localDate">The local date.</param>
        /// <param name="considerShortBreaks">if set to <c>true</c> [consider short breaks].</param>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2009-01-21
        /// </remarks>
        void ResourceCalculateDate(DateOnly localDate, bool considerShortBreaks);

    	/// <summary>
    	/// Creates the skill skill staff dictionary on skills.
    	/// </summary>
    	/// <param name="skillStaffPeriodDictionary">The skill staff period dictionary.</param>
    	/// <param name="skills">The skills.</param>
    	/// <param name="keyPeriod">The key period.</param>
    	/// <returns></returns>
    	ISkillSkillStaffPeriodExtendedDictionary CreateSkillSkillStaffDictionaryOnSkills(
    		ISkillSkillStaffPeriodExtendedDictionary skillStaffPeriodDictionary, IList<ISkill> skills,
    		DateTimePeriod keyPeriod);

	    void ResourceCalculateDate(IResourceCalculationDataContainer relevantProjections,
	                                               DateOnly localDate, bool considerShortBreaks);
    }
}
