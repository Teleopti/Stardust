using System.Globalization;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

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
	    public Gridlock(IPerson person, DateOnly localDate, LockType lockType)
        {
            Person = person;
            LocalDate = localDate;
            LockType = lockType;
        }

	    public Gridlock(IScheduleDay schedulePart, LockType lockType)
	    {
		    Person = schedulePart.Person;
		    LocalDate = schedulePart.DateOnlyAsPeriod.DateOnly;
		    LockType = lockType;
	    }
		
        public IPerson Person { get; }

	    public DateOnly LocalDate { get; }

	    public LockType LockType { get; }
		
        public string Key => Person.GetHashCode().ToString(CultureInfo.InvariantCulture) + "|" + LocalDate.GetHashCode().ToString(CultureInfo.InvariantCulture) + "|" + LockType;
    }   
}
