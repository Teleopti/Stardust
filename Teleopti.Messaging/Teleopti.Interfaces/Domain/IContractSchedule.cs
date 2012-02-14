using System;
using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{

    /// <summary>
    /// ContractSchedule 
    /// </summary>
    public interface IContractSchedule : IAggregateRoot
    {
        /// <summary>
        /// Description of ContractSchedule
        /// </summary>
        Description Description { get; set; }

        /// <summary>
        /// Gets the contained weeks in contract schedule
        /// </summary>
        IEnumerable<IContractScheduleWeek> ContractScheduleWeeks { get; }

        /// <summary>
        /// Adds the contract schedule week.
        /// </summary>
        /// <param name="contractScheduleWeek">The contract schedule week.</param>
        /// <remarks>
        /// Created by:Dumithu Dharmasena
        /// Created date: 2008-04-08
        /// </remarks>
        void AddContractScheduleWeek(IContractScheduleWeek contractScheduleWeek);


        /// <summary>
        /// Removes the contract schedule week.
        /// </summary>
        /// <param name="contractScheduleWeek">The contract schedule week.</param>
        /// <remarks>
        /// Created by:Dumithu Dharmasena
        /// Created date: 2008-04-08
        /// </remarks>
        void RemoveContractScheduleWeek(IContractScheduleWeek contractScheduleWeek);

        /// <summary>
        /// Clears the contract schedule week collection
        /// </summary>
        /// <remarks>
        /// Created by:Dumithu Dharmasena
        /// Created date: 2008-04-08
        /// </remarks>
        void ClearContractScheduleWeeks();


        /// <summary>
        /// Gets the workdays.
        /// </summary>
        /// <param name="firstDayOfWeek">The first day of week.</param>
        /// <param name="startDayOfWeek">The start day of week.</param>
        /// <param name="days">The days.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-08-07
        /// </remarks>
        int GetWorkdays(DayOfWeek firstDayOfWeek, DayOfWeek startDayOfWeek, int days);

        /// <summary>
        /// Gets a value indicating whether this instance is choosable.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is choosable; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-09-09
        /// </remarks>
        bool IsChoosable { get; }

        /// <summary>
        /// Determines whether this day index is a work day.
        /// </summary>
        /// <param name="firstDayOfWeek">The first day of week.</param>
        /// <param name="startDayOfWeek">The start day of week.</param>
        /// <param name="dayIndex">Index of the day.</param>
        /// <returns>
        /// 	<c>true</c> if [is work day] [the specified first day of week]; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-09-29
        /// </remarks>
        bool IsWorkday(DayOfWeek firstDayOfWeek, DayOfWeek startDayOfWeek, int dayIndex);

        /// <summary>
        /// Determines whether the requested date would be a workdate relative to the owner start date.
        /// </summary>
        /// <param name="owningPeriodStartDate">The owning period start date.</param>
        /// <param name="requestedDate">The requested date.</param>
        /// <returns>
        /// 	<c>true</c> if [is work day] [the specified owning period start date]; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-11-03
        /// </remarks>
        bool IsWorkday(DateOnly owningPeriodStartDate, DateOnly requestedDate);
    }
}
