using System.Collections.ObjectModel;

namespace Teleopti.Interfaces.Domain
{

    /// <summary>
    /// A mutable part of a schedule range
    /// </summary>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 2008-08-25
    /// </remarks>
    public interface IScheduleDay : ISchedulePart, IProjectionSource, IOriginator<IScheduleDay>
    {
        /// <summary>
        /// Gets the date only as period.
        /// </summary>
        /// <value>The date only as period.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2010-05-19
        /// </remarks>
        IDateOnlyAsDateTimePeriod DateOnlyAsPeriod { get; }


        /// <summary>
        /// What part of this schedule is most significant.
        /// Will only return proper value if working with schedule days, 
        /// otherwise Unknown is returned.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-09-03
        /// </remarks>
        SchedulePartView SignificantPart();

		/// <summary>
		/// Determines whether this instance is scheduled. IE if SignificantPart is a DayOff, ContractDayOff, MainShift or FullDayAbsence
		/// </summary>
		/// <returns>
		/// 	<c>true</c> if this instance is scheduled; otherwise, <c>false</c>.
		/// </returns>
    	bool IsScheduled();

		/// <summary>
		/// Gets a list of cloned person absences.
		/// </summary>
		/// <param name="includeOutsideActualDay"></param>
		/// <returns></returns>
		/// <remarks>
		/// Created by: rogerkr
		/// Created date: 2010-06-21
		/// </remarks>
		ReadOnlyCollection<IPersonMeeting> PersonMeetingCollection(bool includeOutsideActualDay);

        /// <summary>
        /// Gets a list of cloned person absences.
        /// </summary>
        /// <param name="includeOutsideActualDay"></param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2010-06-21
        /// </remarks>
        ReadOnlyCollection<IPersonAbsence> PersonAbsenceCollection(bool includeOutsideActualDay);

        /// <summary>
        /// Gets a list of cloned person absences.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-05-23
        /// </remarks>
        ReadOnlyCollection<IPersonAbsence> PersonAbsenceCollection();

        ///<summary>
        /// What part of this schedule is most significant for display.
        /// Will only return proper value if working with schedule days, 
        /// otherwise Unknown is returned.
        ///</summary>
        ///<returns></returns>
        SchedulePartView SignificantPartForDisplay();

        #region Shouldn't be here!
        /// <summary>
        /// Delete full day absence
        /// </summary>
        /// <param name="source"></param>
        void DeleteFullDayAbsence(IScheduleDay source);


        /// <summary>
        /// Merge absences
        /// </summary>
        /// <param name="source"></param>
        /// <param name="all"></param>
        void MergeAbsences(IScheduleDay source, bool all);


        /// <summary>
        /// Delete absence
        /// </summary>
        void DeleteAbsence(bool all);

	    /// <summary>
	    /// Does the swapping and updates the SchedulePart represented by the source
	    /// </summary>
	    /// <param name="source"></param>
		/// <param name="isDelete"></param>
	    void Swap(IScheduleDay source, bool isDelete);

        /// <summary>
        /// Updates the SchedulePart from the source
        /// </summary>
        /// <param name="source"></param>
        /// <param name="isDelete"></param>
        void Merge(IScheduleDay source, bool isDelete);

        /// <summary>
        /// Updates the SchedulePart from the source
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="isDelete">if set to <c>true</c> [delete].</param>
        /// <param name="ignoreTimeZoneChanges">if set to <c>true</c> calculate with time changes.</param>
        void Merge(IScheduleDay source, bool isDelete, bool ignoreTimeZoneChanges);

        #endregion
    }

}
