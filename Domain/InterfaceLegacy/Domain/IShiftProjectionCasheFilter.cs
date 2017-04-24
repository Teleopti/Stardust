using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
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
        IList<ShiftProjectionCache> FilterOnRestrictionAndNotAllowedShiftCategories(DateOnly scheduleDayDateOnly, TimeZoneInfo agentTimeZone, IList<ShiftProjectionCache> shiftList,
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
        IList<ShiftProjectionCache> FilterOnRestrictionTimeLimits(DateOnly scheduleDayDateOnly, TimeZoneInfo agentTimeZone, IList<ShiftProjectionCache> shiftList,
                                                                                   IEffectiveRestriction restriction, IWorkShiftFinderResult finderResult);

        /// <summary>
        /// Filters the on date time period.
        /// </summary>
        /// <param name="shiftList">The shift list.</param>
        /// <param name="validPeriod">The valid period.</param>
        /// <param name="finderResult">The finder result.</param>
        /// <returns></returns>
        IList<ShiftProjectionCache> FilterOnDateTimePeriod(IList<ShiftProjectionCache> shiftList, DateTimePeriod validPeriod, IWorkShiftFinderResult finderResult);
        /// <summary>
        /// Filters the on latest start time.
        /// </summary>
        /// <param name="shiftList">The shift list.</param>
        /// <param name="latestStart">The latest start.</param>
        /// <param name="finderResult">The finder result.</param>
        /// <returns></returns>
        IList<ShiftProjectionCache> FilterOnLatestStartTime(IList<ShiftProjectionCache> shiftList, DateTime latestStart, IWorkShiftFinderResult finderResult);
        /// <summary>
        /// Filters the on earliest end time.
        /// </summary>
        /// <param name="shiftList">The shift list.</param>
        /// <param name="earliestEnd">The earliest end.</param>
        /// <param name="finderResult">The finder result.</param>
        /// <returns></returns>
        IList<ShiftProjectionCache> FilterOnEarliestEndTime(IList<ShiftProjectionCache> shiftList, DateTime earliestEnd, IWorkShiftFinderResult finderResult);
        /// <summary>
        /// Filters the on contract time.
        /// </summary>
        /// <param name="validMinMax">The valid min max.</param>
        /// <param name="shiftList">The shift list.</param>
        /// <param name="finderResult">The finder result.</param>
        /// <returns></returns>
        IList<ShiftProjectionCache> FilterOnContractTime(MinMax<TimeSpan> validMinMax, IList<ShiftProjectionCache> shiftList, IWorkShiftFinderResult finderResult);
        /// <summary>
        /// Filters the on restriction min max work time.
        /// </summary>
        /// <param name="shiftList">The shift list.</param>
        /// <param name="restriction">The restriction.</param>
        /// <param name="finderResult">The finder result.</param>
        /// <returns></returns>
        IList<ShiftProjectionCache> FilterOnRestrictionMinMaxWorkTime(IList<ShiftProjectionCache> shiftList, IEffectiveRestriction restriction, IWorkShiftFinderResult finderResult);
        /// <summary>
        /// Filters the on shift category.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="shiftList">The shift list.</param>
        /// <param name="finderResult">The finder result.</param>
        /// <returns></returns>
        IList<ShiftProjectionCache> FilterOnShiftCategory(IShiftCategory category, IList<ShiftProjectionCache> shiftList, IWorkShiftFinderResult finderResult);
        /// <summary>
        /// Filters the on not allowed shift categories.
        /// </summary>
        /// <param name="categories">The categories.</param>
        /// <param name="shiftList">The shift list.</param>
        /// <param name="finderResult">The finder result.</param>
        /// <returns></returns>
        IList<ShiftProjectionCache> FilterOnNotAllowedShiftCategories(IList<IShiftCategory> categories, IList<ShiftProjectionCache> shiftList, IWorkShiftFinderResult finderResult);
        /// <summary>
        /// Filters the on business rules.
        /// </summary>
        /// <param name="current">The current.</param>
        /// <param name="shiftList">The shift list.</param>
        /// <param name="dateToCheck">The date to check.</param>
        /// <param name="finderResult">The finder result.</param>
        /// <returns></returns>
        IList<ShiftProjectionCache> FilterOnBusinessRules(IScheduleRange current, IList<ShiftProjectionCache> shiftList, DateOnly dateToCheck, IWorkShiftFinderResult finderResult);
        /// <summary>
        /// Filters the on start and end time.
        /// </summary>
        /// <param name="startAndEndTime">The start and end time.</param>
        /// <param name="shiftList">The shift list.</param>
        /// <param name="finderResult">The finder result.</param>
        /// <returns></returns>
        IList<ShiftProjectionCache> FilterOnStartAndEndTime(DateTimePeriod startAndEndTime, IList<ShiftProjectionCache> shiftList, IWorkShiftFinderResult finderResult);

        /// <summary>
        /// Filters the specified valid min max.
        /// </summary>
        /// <param name="validMinMax">The valid min max.</param>
        /// <param name="shiftList">The shift list.</param>
        /// <param name="dateToSchedule">The date to schedule.</param>
        /// <param name="current">The current.</param>
        /// <param name="finderResult">The finder result.</param>
        /// <returns></returns>
        IList<ShiftProjectionCache> Filter(IScheduleDictionary schedules, MinMax<TimeSpan> validMinMax, IList<ShiftProjectionCache> shiftList,
                                            DateOnly dateToSchedule, IScheduleRange current,
                                            IWorkShiftFinderResult finderResult);

        /// <summary>
        /// Filters on main shift optimize activities specification.
        /// </summary>
        /// <param name="shiftList">The shift list.</param>
        /// <param name="mainShiftActivitiesOptimizeSpecification"> </param>
        /// <returns></returns>
        IList<ShiftProjectionCache> FilterOnMainShiftOptimizeActivitiesSpecification(
			IList<ShiftProjectionCache> shiftList, ISpecification<IEditableShift> mainShiftActivitiesOptimizeSpecification);

    	///<summary>
    	/// Filter on Business Rules on all Persons in List
    	///</summary>
    	///<param name="groupOfPersons"></param>
    	///<param name="scheduleDictionary"></param>
    	///<param name="shiftList"></param>
    	///<param name="dateOnly"></param>
    	///<param name="finderResult"></param>
    	///<returns></returns>
    	IList<ShiftProjectionCache> FilterOnBusinessRules(IEnumerable<IPerson> groupOfPersons,
    	                                                   IScheduleDictionary scheduleDictionary,
														   DateOnly dateOnly, IList<ShiftProjectionCache> shiftList, 
    	                                                   IWorkShiftFinderResult finderResult);

        
    }
}
