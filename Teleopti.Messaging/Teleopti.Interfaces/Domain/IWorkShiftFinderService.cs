using System;
using System.Collections.Generic;
using Teleopti.Ccc.Secrets.WorkShiftCalculator;

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
    	/// <param name="schedulingOptions">The scheduling options.</param>
    	/// <param name="matrix">The matrix.</param>
    	/// <param name="effectiveRestriction">The effective restriction.</param>
    	/// <param name="possibleStartEndCategory"> </param>
    	/// <returns></returns>
    	/// ///
    	/// <remarks>
    	/// Created by: Ola
    	/// Created date: 2008-09-18
    	/// ///
    	/// </remarks>
    	IWorkShiftCalculationResultHolder FindBestShift(IScheduleDay schedulePart, ISchedulingOptions schedulingOptions, IScheduleMatrixPro matrix, IEffectiveRestriction effectiveRestriction, IPossibleStartEndCategory possibleStartEndCategory);

		/// <summary>
		/// Gets the finder result.
		/// </summary>
		/// <value>The finder result.</value>
		IWorkShiftFinderResult FinderResult { get; }


        /// <summary>
        /// Finds the best main shift.
        /// </summary>
        /// <param name="dateOnly">The date only.</param>
        /// <param name="shiftProjectionCaches">The shift projection caches.</param>
        /// <param name="dataHolders">The data holders.</param>
        /// <param name="maxSeatSkillPeriods">The max seat skill periods.</param>
        /// <param name="nonBlendSkillPeriods">The non blend skill periods.</param>
        /// <param name="currentSchedulePeriod">The current schedule period.</param>
        /// <param name="schedulingOptions">The scheduling options.</param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        IWorkShiftCalculationResultHolder FindBestMainShift(
            DateOnly dateOnly, 
            IList<IShiftProjectionCache> shiftProjectionCaches, 
            //IDictionary<IActivity, IDictionary<DateTime, ISkillStaffPeriodDataHolder>> dataHolders,
			IWorkShiftCalculatorSkillStaffPeriods dataHolders,
			IDictionary<ISkill, ISkillStaffPeriodDictionary> maxSeatSkillPeriods,
            IDictionary<ISkill, ISkillStaffPeriodDictionary> nonBlendSkillPeriods, 
            IVirtualSchedulePeriod currentSchedulePeriod,
            ISchedulingOptions schedulingOptions);
    }
}