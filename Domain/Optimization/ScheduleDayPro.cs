using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

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
	    /// <summary>
        /// Initializes a new instance of the <see cref="ScheduleDayPro"/> class.
        /// </summary>
        /// <param name="day">The day.</param>
        /// <param name="scheduleMatrix">The schedule matrix.</param>
        public ScheduleDayPro(DateOnly day, IScheduleMatrixPro scheduleMatrix)
        {
            InParameter.NotNull(nameof(scheduleMatrix), scheduleMatrix);
            Day = day;
            Parent = scheduleMatrix; 
        }

	    public DateOnly Day { get; }

	    public IScheduleMatrixPro Parent { get; }

	    public IScheduleRange ActiveScheduleRange => Parent.ActiveScheduleRange;

	    public IPerson Person => Parent.Person;

	    public IScheduleDay DaySchedulePart()
        {
            return ActiveScheduleRange.ScheduledDay(Day);
        }
    }
}