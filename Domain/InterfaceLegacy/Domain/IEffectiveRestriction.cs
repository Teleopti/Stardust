using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.Restriction;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    /// <summary>
    /// Represents the restrictions on a workshift
    /// </summary>
    public interface IEffectiveRestriction : IWorkTimeMinMaxRestriction
    {
        /// <summary>
        /// Gets the start time limitation.
        /// </summary>
        /// <value>The start time limitation.</value>
        StartTimeLimitation StartTimeLimitation { get; }

        /// <summary>
        /// Gets the end time limitation.
        /// </summary>
        /// <value>The end time limitation.</value>
        EndTimeLimitation EndTimeLimitation { get; }

        /// <summary>
        /// Gets the work time limitation.
        /// </summary>
        /// <value>The work time limitation.</value>
        WorkTimeLimitation WorkTimeLimitation { get; }

        /// <summary>
        /// Gets the absence
        /// </summary>
        IAbsence Absence { get; set; }

        /// <summary>
        /// Gets the shift category.
        /// </summary>
        /// <value>The shift category.</value>
        IShiftCategory ShiftCategory { get; set; }

        /// <summary>
        /// Gets the day off.
        /// </summary>
        /// <value>The day off.</value>
        IDayOffTemplate DayOffTemplate { get; set; }

        /// <summary>
        /// Combines the specified effective restriction.
        /// </summary>
        /// <param name="effectiveRestriction">The effective restriction.</param>
        /// <returns></returns>
        IEffectiveRestriction Combine(IEffectiveRestriction effectiveRestriction);

        /// <summary>
        /// Determines whether [is rotation day].
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if [is rotation day]; otherwise, <c>false</c>.
        /// </returns>
        bool IsRotationDay { get; set; }

        /// <summary>
        /// Determines whether [is availability day].
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if [is availability day]; otherwise, <c>false</c>.
        /// </returns>
        bool IsAvailabilityDay { get; set; }

        /// <summary>
        /// Determines whether [is preference day].
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if [is preference day]; otherwise, <c>false</c>.
        /// </returns>
        bool IsPreferenceDay { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is student availability day.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is student availability day; otherwise, <c>false</c>.
        /// </value>
        bool IsStudentAvailabilityDay { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [not available].
        /// </summary>
        /// <value><c>true</c> if [not available]; otherwise, <c>false</c>.</value>
        bool NotAvailable { get; set; }

        ///<summary>
        /// Gets the ActivityRestrictionCollection
        ///</summary>
        IList<IActivityRestriction> ActivityRestrictionCollection { get; }

        /// <summary>
        /// Visuals the layer collection satisfies activity restriction.
        /// </summary>
        /// <param name="scheduleDayDateOnly">The schedule day date only.</param>
        /// <param name="agentTimeZone">The agent time zone.</param>
        /// <param name="layers">The layers.</param>
        /// <returns></returns>
        bool VisualLayerCollectionSatisfiesActivityRestriction(DateOnly scheduleDayDateOnly,
                                                               TimeZoneInfo agentTimeZone,
															   IEnumerable<IActivityRestrictableVisualLayer> layers);

       
        /// <summary>
        /// Gets a value indicating whether this instance is restriction.
        /// returns true if one one more of IsPreferenceDay, IsRotationDay, IsAvailabilityDay or IsStudentAvailabilityDay is true
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is restriction; otherwise, <c>false</c>.
        /// </value>
        bool IsRestriction { get; }

    	/// <summary>
    	/// If true we should not add a contract day off
    	/// </summary>
    	bool NotAllowedForDayOffs { get; set; }

		/// <summary>
		/// The common main shift to be assigned to all the rest free slots
		/// </summary>
		IEditableShift CommonMainShift { get; set; }

		/// <summary>
		/// The common activity for all shifts
		/// </summary>
		CommonActivity CommonActivity { get; set; }
    }
}