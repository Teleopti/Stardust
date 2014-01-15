using Teleopti.Interfaces.Domain;

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

        #region Variables

        private DateOnly _day;
        private IScheduleMatrixPro _scheduleMatrix;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ScheduleDayPro"/> class.
        /// </summary>
        /// <param name="day">The day.</param>
        /// <param name="scheduleMatrix">The schedule matrix.</param>
        public ScheduleDayPro(DateOnly day, IScheduleMatrixPro scheduleMatrix)
        {
            InParameter.NotNull("scheduleMatrix", scheduleMatrix);
            _day = day;
            _scheduleMatrix = scheduleMatrix; 
        }

        #endregion

        #region Interface

        public DateOnly Day
        {
            get { return _day; }
        }

        public IScheduleMatrixPro Parent
        {
            get { return _scheduleMatrix; }
        }

        public IScheduleRange ActiveScheduleRange
        {
            get { return Parent.ActiveScheduleRange; }
        }

        public IPerson Person
        {
            get
            {
                return Parent.Person;
            }
        }

        public IScheduleDay DaySchedulePart()
        {
            return ActiveScheduleRange.ScheduledDay(Day);
        }

        #endregion

    }
}