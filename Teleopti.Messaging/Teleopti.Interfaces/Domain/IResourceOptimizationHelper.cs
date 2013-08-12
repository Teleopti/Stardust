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
        /// <param name="useOccupancyAdjustment">if set to <c>true</c> use occupancy adjustment.</param>
        /// <param name="considerShortBreaks">if set to <c>true</c> [consider short breaks].</param>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2009-01-21
        /// </remarks>
        void ResourceCalculateDate(DateOnly localDate, bool useOccupancyAdjustment, bool considerShortBreaks);


		/// <summary>
		/// Resources the calculate date.
		/// </summary>
		/// <param name="localDate">The local date.</param>
		/// <param name="useOccupancyAdjustment">if set to <c>true</c> [use occupancy adjustment].</param>
		/// <param name="considerShortBreaks">if set to <c>true</c> [consider short breaks].</param>
		/// <param name="toRemove">To remove.</param>
		/// <param name="toAdd">To add.</param>
    	void ResourceCalculateDate(DateOnly localDate, bool useOccupancyAdjustment, bool considerShortBreaks,
								   IEnumerable<IScheduleDay> toRemove, IEnumerable<IScheduleDay> toAdd);

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
	                                               DateOnly localDate, bool useOccupancyAdjustment, bool considerShortBreaks);
    }
}
