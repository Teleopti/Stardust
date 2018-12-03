using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    /// <summary>
    /// The schedule for one person during a period of time.
    /// </summary>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 2008-08-25
    /// </remarks>
    public interface IScheduleRange : ISchedule
    {
		bool CanSeeUnpublishedSchedules { get; set; }

		DateTimePeriod? TotalPeriod();

		/// <summary>
		/// Extracts all schedule data including data with no permission.
		/// The extractor is responsible of keeping all this data internal!
		/// </summary>
		/// <param name="extractor">The extractor.</param>
		/// <param name="period">The period.</param>
		/// <remarks>
		/// Created by: rogerkr
		/// Created date: 2008-05-19
		/// </remarks>
		void ExtractAllScheduleData(IScheduleExtractor extractor, DateTimePeriod period);

        /// <summary>
        /// Gets the cloned schedule for a certain day
        /// </summary>
        /// <param name="day">The date.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-12-17
        /// </remarks>
        IScheduleDay ScheduledDay(DateOnly day);

        /// <summary>
        /// Determines whether [contains] [the specified schedule data].
        /// </summary>
        /// <param name="scheduleData">The schedule data.</param>
        /// <param name="includeNonPermitted">if set to <c>true</c> [include unpermitted].</param>
        /// <returns>
        /// 	<c>true</c> if [contains] [the specified schedule data]; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2010-02-11
        /// </remarks>
        bool Contains(IScheduleData scheduleData, bool includeNonPermitted);
		bool Contains(IScheduleData scheduleData);


		TimeSpan CalculatedContractTimeHolderOnPeriod(DateOnlyPeriod periodToCheck);

	    /// <summary>
	    /// Gets or sets the calculated target time holder. Used by the result columns in Scheduler.
	    /// This field vill be set to null when Modify method is called.
	    /// </summary>
	    /// <value>The calculated target time holder.</value>
	    /// <remarks>
	    /// Created by: micke
	    /// Created date: 2008-11-21
	    /// </remarks>
	    TimeSpan? CalculatedTargetTimeHolder(DateOnlyPeriod periodToCheck);

		TimeSpan? CalculatedTargetTime(DateOnlyPeriod periodToCheck);

	    /// <summary>
	    /// Gets or sets the calculated target schedule days off. Used by the result columns in Scheduler.
	    /// This field vill be set to null when Modify method is called.
	    /// </summary>
	    /// <value>The calculated target schedule days off.</value>
	    /// <remarks>
	    /// Created by: micke
	    /// Created date: 2008-11-22
	    /// </remarks>
	    int? CalculatedTargetScheduleDaysOff(DateOnlyPeriod periodToCheck);

	    int CalculatedScheduleDaysOffOnPeriod(DateOnlyPeriod periodToCheck);

        /// <summary>
        /// Scheduleds the day colletion.
        /// </summary>
        /// <param name="dateOnlyPeriod">The date only period.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2010-05-25
        /// </remarks>
        IEnumerable<IScheduleDay> ScheduledDayCollection(DateOnlyPeriod dateOnlyPeriod);

		/// <summary>
		/// Get scheduleds for the day colletion to get student availability
		/// This method will ignore published date and view unpublished schdule permission setting to get 
		/// student availability after published date even if current user has no permission to view unpublished schedule.
		/// It should only be used to get schedule to retrieve student availability.
		/// Refer to bug #33327: Agents can no longer see Availability they entered for dates that have not been published.
		/// </summary>
		/// <param name="dateOnlyPeriod">The date only period.</param>
		/// <returns></returns>
		/// <remarks>
		/// Created by: xinfengl
		/// Created date: 2015-05-21
		/// </remarks>
		IEnumerable<IScheduleDay> ScheduledDayCollectionNoViewPublishedScheduleCheck(DateOnlyPeriod dateOnlyPeriod);

        /// <summary>
        /// Refetches the schedulepart
        /// </summary>
        /// <param name="schedulePart">The schedule part.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2010-05-24
        /// </remarks>
        IScheduleDay ReFetch(IScheduleDay schedulePart);

        /// <summary>
        /// Gets the shift category fairness.
        /// </summary>
        /// <returns></returns>
        IShiftCategoryFairnessHolder CachedShiftCategoryFairness();

		/// <summary>
		/// What is the (potential) changes on this schedule range?
		/// </summary>
		/// <param name="differenceService"></param>
		/// <returns></returns>
		IDifferenceCollection<IPersistableScheduleData> DifferenceSinceSnapshot(IDifferenceCollectionService<IPersistableScheduleData> differenceService);

		/// <summary>
		/// What is the (potential) changes on this schedule range?
		/// </summary>
		/// <param name="differenceService"></param>
		/// <param name="period"></param>
		/// <returns></returns>
		IDifferenceCollection<IPersistableScheduleData> DifferenceSinceSnapshot(IDifferenceCollectionService<IPersistableScheduleData> differenceService, DateOnlyPeriod period);

		/// <summary>
		/// Tells the domain that current state corresponds with db state
		/// </summary>
    	void TakeSnapshot();

    	/// <summary>
    	/// Added to be able to view unpublished data in reports
    	/// </summary>
    	/// <param name="day"></param>
    	/// <param name="includeUnpublished"></param>
    	/// <returns></returns>
    	IScheduleDay ScheduledDay(DateOnly day, bool includeUnpublished);

	    void ForceRecalculationOfTargetTimeContractTimeAndDaysOff();

	    bool IsEmpty();
	    TargetScheduleSummary CalculatedTargetTimeSummary(DateOnlyPeriod periodToCheck);
	    CurrentScheduleSummary CalculatedCurrentScheduleSummary(DateOnlyPeriod periodToCheck);
		void CopyTo(IScheduleRange scheduleRangeToModify);
	}
}
