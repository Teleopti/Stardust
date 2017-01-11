﻿using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    /// <summary>
    /// Daily data holder class for representing a selected schedule day in a selected schedule period.
    /// </summary>
    /// <remarks>
    /// Created by: tamasb
    /// Created date: 2008-08-21
    /// </remarks>
    public class ScheduleDayPro : IScheduleDayPro
    {
	    private readonly DateOnly _day;
        private readonly IScheduleMatrixPro _scheduleMatrix;

	    /// <summary>
        /// Initializes a new instance of the <see cref="ScheduleDayPro"/> class.
        /// </summary>
        /// <param name="day">The day.</param>
        /// <param name="scheduleMatrix">The schedule matrix.</param>
        public ScheduleDayPro(DateOnly day, IScheduleMatrixPro scheduleMatrix)
        {
            InParameter.NotNull(nameof(scheduleMatrix), scheduleMatrix);
            _day = day;
            _scheduleMatrix = scheduleMatrix; 
        }

	    public DateOnly Day => _day;

	    public IScheduleMatrixPro Parent => _scheduleMatrix;

	    public IScheduleRange ActiveScheduleRange => Parent.ActiveScheduleRange;

	    public IPerson Person => Parent.Person;

	    public IScheduleDay DaySchedulePart()
        {
            return ActiveScheduleRange.ScheduledDay(Day);
        }
    }
}