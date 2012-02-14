using System;
using System.Collections.Generic;
using System.Text;
using Teleopti.Ccc.Sdk.Client.SdkServiceReference;

namespace Teleopti.Ccc.AgentPortalCode.Common.Factory
{
    /// <summary>
    /// Referesent the builder classs which responsible to build different types of Preference restrictions
    /// </summary>
    /// <remarks>
    /// Created by: Sumedah
    /// Created date: 2008-11-10
    /// </remarks>
    public static class ScheduleRestrictionFactory
    {

        #region Methods - Static Members

        #region Methods - Static Members - ScheduleRestrictionFactory Memebrs

        ///// <summary>
        ///// Creates the schedule restriction.
        ///// </summary>
        ///// <param name="scheduleRestrictionType">Type of the schedule restriction.</param>
        ///// <returns></returns>
        ///// <remarks>
        ///// Created by: Sumedah
        ///// Created date: 2008-11-10
        ///// </remarks>
        //public static PreferenceRestrictionDto Create(Dto scheduleRestrictionType)
        //{
        //    PreferenceRestrictionDto returnScheduleRestrictionDto = null;

        //    if(scheduleRestrictionType is ShiftCategoryDto )
        //    {
        //        returnScheduleRestrictionDto = CreateShiftCategoryPreferenceRestriction(scheduleRestrictionType);
        //    }
        //    else if (scheduleRestrictionType is DayOffInfoDto)
        //    {
        //        returnScheduleRestrictionDto = CreateDayOffPreferenceRestriction(scheduleRestrictionType);
        //    }
        //    else if (scheduleRestrictionType is ActivityDto)
        //    {
        //        returnScheduleRestrictionDto = CreateActivityPreferenceRestriction(scheduleRestrictionType);
        //    }

        //    return returnScheduleRestrictionDto;
        //}

        ///// <summary>
        ///// Creates the availability restriction.
        ///// </summary>
        ///// <param name="startTime">The start time string.</param>
        ///// <param name="endTime">The end time string.</param>
        ///// <returns></returns>
        ///// <remarks>
        ///// Created by: MadhurangaP
        ///// Created date: 2008-12-18
        ///// </remarks>
        //public static AvailabilityRestrictionDto CreateAvailabilityRestriction(string startTime, string endTime)
        //{
        //    AvailabilityRestrictionDto availabilityRestrictionDto = new AvailabilityRestrictionDto();

        //    availabilityRestrictionDto.Available = true;
        //    availabilityRestrictionDto.AvailableSpecified = true;
        //    availabilityRestrictionDto.RestrictionType = ScheduleRestrictionDtoType.Availability;
        //    availabilityRestrictionDto.STLimitationStartTimeString = startTime;
        //    availabilityRestrictionDto.ETLimitationEndTimeString = endTime;

        //    return availabilityRestrictionDto;
        //}

        #endregion Methods

        #region Methods - Static Members - ScheduleRestrictionFactory Members -  (helpers)

        ///// <summary>
        ///// Creates the shift category preference restriction.
        ///// </summary>
        ///// <param name="selectedShiftCategory">The selected shift category.</param>
        ///// <returns></returns>
        ///// <remarks>
        ///// Created by: Sumedah
        ///// Created date: 2008-11-10
        ///// </remarks>
        //private static PreferenceRestrictionDto CreateShiftCategoryPreferenceRestriction(Dto selectedShiftCategory)
        //{
        //    PreferenceRestrictionDto preferenceRestrictionDto = new PreferenceRestrictionDto();

        //    preferenceRestrictionDto.ShiftCategory = selectedShiftCategory as ShiftCategoryDto;
        //    preferenceRestrictionDto.DayOff = null;
        //    preferenceRestrictionDto.Activity = null;

        //    return preferenceRestrictionDto;
        //}

        ///// <summary>
        ///// Creates the day off preference restriction.
        ///// </summary>
        ///// <param name="selectedDayOff">The selected day off.</param>
        ///// <returns></returns>
        ///// <remarks>
        ///// Created by: Sumedah
        ///// Created date: 2008-11-10
        ///// </remarks>
        //private static PreferenceRestrictionDto CreateDayOffPreferenceRestriction(Dto selectedDayOff)
        //{
        //    PreferenceRestrictionDto preferenceRestrictionDto = new PreferenceRestrictionDto();

        //    preferenceRestrictionDto.ShiftCategory = null;
        //    preferenceRestrictionDto.DayOff = selectedDayOff as DayOffInfoDto;
        //    preferenceRestrictionDto.Activity = null;

        //    return preferenceRestrictionDto;
        //}

        ///// <summary>
        ///// Creates the activity preference restriction.
        ///// </summary>
        ///// <param name="selectedActivity">The selected activity.</param>
        ///// <returns></returns>
        ///// <remarks>
        ///// Created by: Sumedah
        ///// Created date: 2008-11-10
        ///// </remarks>
        //private static PreferenceRestrictionDto CreateActivityPreferenceRestriction(Dto selectedActivity)
        //{
        //    PreferenceRestrictionDto preferenceRestrictionDto = new PreferenceRestrictionDto();

        //    preferenceRestrictionDto.ShiftCategory = null;
        //    preferenceRestrictionDto.DayOff = null;
        //    preferenceRestrictionDto.Activity = selectedActivity as ActivityDto;

        //    return preferenceRestrictionDto;
        //}

        #endregion Methods

        #endregion Methods

    }
}
