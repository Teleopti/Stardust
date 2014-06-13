using System;
using System.Collections.Generic;


namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// 
    /// </summary>
    public interface IShiftProjectionCacheFilter
    {
        /// <summary>
        /// Checks the restrictions.
        /// </summary>
        /// <param name="schedulingOptions">The scheduling options.</param>
        /// <param name="effectiveRestriction">The effective restriction.</param>
        /// <param name="finderResult">The finder result.</param>
        /// <returns></returns>
        bool CheckRestrictions(ISchedulingOptions schedulingOptions, IEffectiveRestriction effectiveRestriction, IWorkShiftFinderResult finderResult);

        /// <summary>
        /// Filters the on restriction and not allowed shift categories.
        /// </summary>
        /// <param name="scheduleDayDateOnly">The schedule day date only.</param>
        /// <param name="agentTimeZone">The agent time zone.</param>
        /// <param name="shiftList">The shift list.</param>
        /// <param name="restriction">The restriction.</param>
        /// <param name="notAllowedCategories">The not allowed categories.</param>
        /// <param name="finderResult">The finder result.</param>
        /// <returns></returns>
        IList<IShiftProjectionCache> FilterOnRestrictionAndNotAllowedShiftCategories(DateOnly scheduleDayDateOnly, TimeZoneInfo agentTimeZone, IList<IShiftProjectionCache> shiftList,
                                                                                   IEffectiveRestriction restriction, IList<IShiftCategory> notAllowedCategories, IWorkShiftFinderResult finderResult);

        /// <summary>
        /// Filters the on restriction time limits.
        /// </summary>
        /// <param name="scheduleDayDateOnly">The schedule day date only.</param>
        /// <param name="agentTimeZone">The agent time zone.</param>
        /// <param name="shiftList">The shift list.</param>
        /// <param name="restriction">The restriction.</param>
        /// <param name="finderResult">The finder result.</param>
        /// <returns></returns>
        IList<IShiftProjectionCache> FilterOnRestrictionTimeLimits(DateOnly scheduleDayDateOnly, TimeZoneInfo agentTimeZone, IList<IShiftProjectionCache> shiftList,
                                                                                   IEffectiveRestriction restriction, IWorkShiftFinderResult finderResult);

        /// <summary>
        /// Filters the on date time period.
        /// </summary>
        /// <param name="shiftList">The shift list.</param>
        /// <param name="validPeriod">The valid period.</param>
        /// <param name="finderResult">The finder result.</param>
        /// <returns></returns>
        IList<IShiftProjectionCache> FilterOnDateTimePeriod(IList<IShiftProjectionCache> shiftList, DateTimePeriod validPeriod, IWorkShiftFinderResult finderResult);
        /// <summary>
        /// Filters the on latest start time.
        /// </summary>
        /// <param name="shiftList">The shift list.</param>
        /// <param name="latestStart">The latest start.</param>
        /// <param name="finderResult">The finder result.</param>
        /// <returns></returns>
        IList<IShiftProjectionCache> FilterOnLatestStartTime(IList<IShiftProjectionCache> shiftList, DateTime latestStart, IWorkShiftFinderResult finderResult);
        /// <summary>
        /// Filters the on earliest end time.
        /// </summary>
        /// <param name="shiftList">The shift list.</param>
        /// <param name="earliestEnd">The earliest end.</param>
        /// <param name="finderResult">The finder result.</param>
        /// <returns></returns>
        IList<IShiftProjectionCache> FilterOnEarliestEndTime(IList<IShiftProjectionCache> shiftList, DateTime earliestEnd, IWorkShiftFinderResult finderResult);
        /// <summary>
        /// Filters the on contract time.
        /// </summary>
        /// <param name="validMinMax">The valid min max.</param>
        /// <param name="shiftList">The shift list.</param>
        /// <param name="finderResult">The finder result.</param>
        /// <returns></returns>
        IList<IShiftProjectionCache> FilterOnContractTime(MinMax<TimeSpan> validMinMax, IList<IShiftProjectionCache> shiftList, IWorkShiftFinderResult finderResult);
        /// <summary>
        /// Filters the on restriction min max work time.
        /// </summary>
        /// <param name="shiftList">The shift list.</param>
        /// <param name="restriction">The restriction.</param>
        /// <param name="finderResult">The finder result.</param>
        /// <returns></returns>
        IList<IShiftProjectionCache> FilterOnRestrictionMinMaxWorkTime(IList<IShiftProjectionCache> shiftList, IEffectiveRestriction restriction, IWorkShiftFinderResult finderResult);
        /// <summary>
        /// Filters the on shift category.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="shiftList">The shift list.</param>
        /// <param name="finderResult">The finder result.</param>
        /// <returns></returns>
        IList<IShiftProjectionCache> FilterOnShiftCategory(IShiftCategory category, IList<IShiftProjectionCache> shiftList, IWorkShiftFinderResult finderResult);
        /// <summary>
        /// Filters the on not allowed shift categories.
        /// </summary>
        /// <param name="categories">The categories.</param>
        /// <param name="shiftList">The shift list.</param>
        /// <param name="finderResult">The finder result.</param>
        /// <returns></returns>
        IList<IShiftProjectionCache> FilterOnNotAllowedShiftCategories(IList<IShiftCategory> categories, IList<IShiftProjectionCache> shiftList, IWorkShiftFinderResult finderResult);
        /// <summary>
        /// Filters the on business rules.
        /// </summary>
        /// <param name="current">The current.</param>
        /// <param name="shiftList">The shift list.</param>
        /// <param name="dateToCheck">The date to check.</param>
        /// <param name="finderResult">The finder result.</param>
        /// <returns></returns>
        IList<IShiftProjectionCache> FilterOnBusinessRules(IScheduleRange current, IList<IShiftProjectionCache> shiftList, DateOnly dateToCheck, IWorkShiftFinderResult finderResult);
        /// <summary>
        /// Filters the on start and end time.
        /// </summary>
        /// <param name="startAndEndTime">The start and end time.</param>
        /// <param name="shiftList">The shift list.</param>
        /// <param name="finderResult">The finder result.</param>
        /// <returns></returns>
        IList<IShiftProjectionCache> FilterOnStartAndEndTime(DateTimePeriod startAndEndTime, IList<IShiftProjectionCache> shiftList, IWorkShiftFinderResult finderResult);

        /// <summary>
        /// Filters the specified valid min max.
        /// </summary>
        /// <param name="validMinMax">The valid min max.</param>
        /// <param name="shiftList">The shift list.</param>
        /// <param name="dateToSchedule">The date to schedule.</param>
        /// <param name="current">The current.</param>
        /// <param name="finderResult">The finder result.</param>
        /// <returns></returns>
        IList<IShiftProjectionCache> Filter(MinMax<TimeSpan> validMinMax, IList<IShiftProjectionCache> shiftList,
                                            DateOnly dateToSchedule, IScheduleRange current,
                                            IWorkShiftFinderResult finderResult);

        /// <summary>
        /// Filters on main shift optimize activities specification.
        /// </summary>
        /// <param name="shiftList">The shift list.</param>
        /// <param name="mainShiftActivitiesOptimizeSpecification"> </param>
        /// <returns></returns>
        IList<IShiftProjectionCache> FilterOnMainShiftOptimizeActivitiesSpecification(
			IList<IShiftProjectionCache> shiftList, ISpecification<IEditableShift> mainShiftActivitiesOptimizeSpecification);

    	///<summary>
    	/// Filter on Business Rules on all Persons in List
    	///</summary>
    	///<param name="groupOfPersons"></param>
    	///<param name="scheduleDictionary"></param>
    	///<param name="shiftList"></param>
    	///<param name="dateOnly"></param>
    	///<param name="finderResult"></param>
    	///<returns></returns>
    	IList<IShiftProjectionCache> FilterOnBusinessRules(IEnumerable<IPerson> groupOfPersons,
    	                                                   IScheduleDictionary scheduleDictionary,
														   DateOnly dateOnly, IList<IShiftProjectionCache> shiftList, 
    	                                                   IWorkShiftFinderResult finderResult);

        /// <summary>
        /// Filter on activities which cannot be overwritten by meetings and personal shifts
        /// </summary>
        /// <param name="shiftList"></param>
        /// <param name="part"></param>
        /// <param name="finderResult"></param>
        /// <returns></returns>
        IList<IShiftProjectionCache> FilterOnNotOverWritableActivities(IList<IShiftProjectionCache> shiftList,
                                                                    IScheduleDay part,
                                                                    IWorkShiftFinderResult finderResult);
    }
}
