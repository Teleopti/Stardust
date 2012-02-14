using System;
using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Service for finding a workshift.
    /// </summary>
    public interface IWorkShiftFinderService
    {
        /// <summary>
        /// Finds the best shift.
        /// </summary>
        /// <param name="schedulePart">The schedule part.</param>
        /// <param name="effectiveRestriction">The effective restriction.</param>
        /// <param name="matrix"></param>
        /// <returns></returns>
        /// ///
        /// <remarks>
        /// Created by: Ola
        /// Created date: 2008-09-18
        /// ///
        /// </remarks>
        IWorkShiftCalculationResultHolder FindBestShift(IScheduleDay schedulePart, IEffectiveRestriction effectiveRestriction, IScheduleMatrixPro matrix);

		/// <summary>
		/// Gets the finder result.
		/// </summary>
		/// <value>The finder result.</value>
		IWorkShiftFinderResult FinderResult { get; }


        /// <summary>
        /// Finds the best main shift.
        /// </summary>
        /// <param name="dateOnly"></param>
        /// <param name="shiftProjectionCaches">The shift projection caches.</param>
        /// <param name="dataHolders">The data holders.</param>
        /// <param name="maxSeatSkillPeriods"></param>
        /// <param name="nonBlendSkillPeriods"></param>
        /// <param name="currentSchedulePeriod"></param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        IWorkShiftCalculationResultHolder FindBestMainShift(
            DateOnly dateOnly, 
            IList<IShiftProjectionCache> shiftProjectionCaches, 
            IDictionary<IActivity, IDictionary<DateTime, ISkillStaffPeriodDataHolder>> dataHolders,
			IDictionary<ISkill, ISkillStaffPeriodDictionary> maxSeatSkillPeriods,
            IDictionary<ISkill, ISkillStaffPeriodDictionary> nonBlendSkillPeriods, 
            IVirtualSchedulePeriod currentSchedulePeriod);
    }
}