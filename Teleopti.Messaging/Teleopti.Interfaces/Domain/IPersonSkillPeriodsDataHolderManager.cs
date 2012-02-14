using System;
using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// 
    /// </summary>
    public interface IPersonSkillPeriodsDataHolderManager
    {
        /// <summary>
        /// Gets the person skill periods data holder dictionary.
        /// </summary>
        /// <param name="scheduleDateOnly">The schedule date only.</param>
		/// <param name="currentSchedulePeriod">The Virtual Schedule Period.</param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		IDictionary<IActivity, IDictionary<DateTime, ISkillStaffPeriodDataHolder>> GetPersonSkillPeriodsDataHolderDictionary(DateOnly scheduleDateOnly, IVirtualSchedulePeriod currentSchedulePeriod);

		/// <summary>
		/// Gets the person MaxSeatSkill SkillStaffPeriods.
		/// </summary>
		/// <param name="scheduleDateOnly">The schedule date only.</param>
		/// <param name="currentSchedulePeriod">The current schedule period.</param>
		/// <returns></returns>
    	IDictionary<ISkill, ISkillStaffPeriodDictionary> GetPersonMaxSeatSkillSkillStaffPeriods(DateOnly scheduleDateOnly,
    	                                                                                        IVirtualSchedulePeriod
    	                                                                                        	currentSchedulePeriod);

        /// <summary>
        /// Gets the person NonBlendSkill SkillStaffPeriods.
        /// </summary>
        /// <param name="scheduleDateOnly">The schedule date only.</param>
        /// <param name="currentSchedulePeriod">The current schedule period.</param>
        /// <returns></returns>
        IDictionary<ISkill, ISkillStaffPeriodDictionary> GetPersonNonBlendSkillSkillStaffPeriods(DateOnly scheduleDateOnly,
                                                                                                IVirtualSchedulePeriod
                                                                                                    currentSchedulePeriod);
    }
}
