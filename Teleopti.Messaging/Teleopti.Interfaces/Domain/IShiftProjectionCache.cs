using System;
using System.Collections.ObjectModel;
using Teleopti.Ccc.Secrets.WorkShiftCalculator;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Object fo caching a shift projection.
    /// </summary>
	public interface IShiftProjectionCache : IWorkShiftCalculatableProjection
    {
        /// <summary>
        /// Sets the date.
        /// </summary>
        /// <param name="schedulingDate">The scheduling date.</param>
        /// <param name="localTimeZoneInfo">The local time zone info.</param>
        void SetDate(DateOnly schedulingDate, TimeZoneInfo localTimeZoneInfo);

        /// <summary>
        /// Gets the main shift.
        /// </summary>
        /// <value>The main shift.</value>
        /// /// 
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-09-16    
        /// /// </remarks>
        IEditableShift TheMainShift { get; }

        /// <summary>
        /// Gets the work shift.
        /// </summary>
        /// <value>The work shift.</value>
        /// /// 
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-09-16    
        /// /// </remarks>
        IWorkShift TheWorkShift { get; }


        /// <summary>
        /// Gets the work shift projection contract time.
        /// </summary>
        TimeSpan WorkShiftProjectionContractTime { get; }

        /// <summary>
        /// Gets the work shift projection period.
        /// </summary>
        DateTimePeriod WorkShiftProjectionPeriod { get; }

        /// <summary>
        /// Gets the main shift projection.
        /// </summary>
        /// <value>The main shift projection.</value>
        /// /// 
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-09-16    
        /// /// </remarks>
        IVisualLayerCollection MainShiftProjection { get; }

        /// <summary>
        /// Gets the day of week.
        /// </summary>
        /// <value>The day of week.</value>
        DayOfWeek DayOfWeek { get; }

        /// <summary>
        /// Personals the shifts and meetings are in work time.
        /// </summary>
        /// <param name="meetings">The meetings.</param>
        /// <param name="personAssignments">The person assignments.</param>
        /// <returns></returns>
        bool PersonalShiftsAndMeetingsAreInWorkTime(ReadOnlyCollection<IPersonMeeting> meetings,
                                                    IPersonAssignment personAssignment);

        /// <summary>
        /// Get the Shift Category Justice Value for the day in the week.
        /// </summary>
        /// <returns>The Justice Value for the ShiftCategory</returns>
        int ShiftCategoryDayOfWeekJusticeValue { get; }

    	/// <summary>
    	/// 
    	/// </summary>
    	TimeSpan WorkShiftStartTime { get; }
    	/// <summary>
    	/// 
    	/// </summary>
		TimeSpan WorkShiftEndTime { get; }
    }
}