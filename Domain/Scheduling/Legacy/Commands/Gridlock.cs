using System.Globalization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
    public enum LockType
    {
        /// <summary>
        /// Normal lock
        /// </summary>
        Normal,
        /// <summary>
        /// Authorizaton lock
        /// </summary>
        Authorization,
        /// <summary>
        /// Period lock
        /// </summary>
        OutsidePersonPeriod,
        /// <summary>
        /// UnpublishedSchedule
        /// </summary>
        UnpublishedSchedule,
        /// <summary>
        /// WriteProtected
        /// </summary>
        WriteProtected

    }

    /// <summary>
    /// Lock in grid
    /// </summary>
    public class Gridlock
    {
        private IPerson _person;
        private DateOnly _localDate;
        //private bool _authorizationLock;
        private LockType _lockType;
        private DateTimePeriod _period;
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="localDate">The local date.</param>
        /// <param name="lockType">Type of the lock.</param>
        /// <param name="period">The period.</param>
        public Gridlock(IPerson person, DateOnly localDate, LockType lockType, DateTimePeriod period)
        {
            _person = person;
            _localDate = localDate;
            _lockType = lockType;
            _period = period;
        }

        public Gridlock(IScheduleDay schedulePart, LockType lockType)
        {
            _person = schedulePart.Person;
            _localDate = schedulePart.DateOnlyAsPeriod.DateOnly;
             _lockType = lockType;
            _period = schedulePart.DateOnlyAsPeriod.Period();
        }

        /// <summary>
        /// Person to be locked
        /// </summary>
        public IPerson Person
        {
            get { return _person; }
        }

        /// <summary>
        /// Local date
        /// </summary>
        public DateOnly LocalDate
        {
            get { return _localDate; }
        }

        /// <summary>
        /// Authorization lock
        /// </summary>
        //public bool AuthorizationLock
        //{
        //    get { return _authorizationLock; }
        //}

        /// <summary>
        /// Lock type
        /// </summary>
        public LockType LockType
        {
            get { return _lockType; }
        }

        /// <summary>
        /// return key
        /// </summary>
        public string Key
        {
			get { return _person.GetHashCode().ToString(CultureInfo.InvariantCulture) + "|" + _localDate.GetHashCode().ToString(CultureInfo.InvariantCulture) + "|" + LockType; }
        }

        /// <summary>
        /// Gets the period.
        /// </summary>
        /// <value>The period.</value>
        /// /// 
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2009-04-06    
        /// /// </remarks>
        public DateTimePeriod Period
        {
            get { return _period; }
        }
    }   
}
