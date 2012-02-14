
namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Generic interface for entities with restrictions
    /// </summary>
    public interface IRestrictionChecker<T> : IRestrictionChecker where T : IAggregateRoot
    {
        /// <summary>
        /// Gets the restriction set.
        /// </summary>
        /// <value>The restriction set.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-11-08
        /// </remarks>
        IRestrictionSet<T> RestrictionSet { get; }
    }

    /// <summary>
    /// Interface for entities with restrictions
    /// </summary>
    public interface IRestrictionChecker
    {
        /// <summary>
        /// Checks the restrictions.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-11-07
        /// </remarks>
        void CheckRestrictions();
    }

    /// <summary>
    /// Enum PermissionState
    /// </summary>
    public enum PermissionState
    {
        /// <summary>
        /// Satisfied
        /// </summary>
        Satisfied, 
        /// <summary>
        /// Broken
        /// </summary>
        Broken, 
        /// <summary>
        /// Unspecified
        /// </summary>
        Unspecified,
        /// <summary>
        /// None
        /// </summary>
        None
    }

    /// <summary>
    /// Interface for RestrictionChecker
    /// </summary>
    public interface ICheckerRestriction
    {
        /// <summary>
        /// Check availability
        /// </summary>
        /// <returns></returns>
        PermissionState CheckAvailability();

        /// <summary>
        /// Check rotations
        /// </summary>
        /// <returns></returns>
        PermissionState CheckRotations();

        /// <summary>
        /// Check student availability
        /// </summary>
        /// <returns></returns>
        PermissionState CheckStudentAvailability();

        /// <summary>
        /// Check preference
        /// </summary>
        /// <returns></returns>
        PermissionState CheckPreference();

        /// <summary>
        /// Check preference must have
        /// </summary>
        /// <returns></returns>
        PermissionState CheckPreferenceMustHave();

        /// <summary>
        /// Check preference absence
        /// </summary>
        /// <param name="permissionState"></param>
        /// <returns></returns>
        PermissionState CheckPreferenceAbsence(PermissionState permissionState);

        /// <summary>
        /// Check preference Day Off
        /// </summary>
        /// <returns></returns>
        PermissionState CheckPreferenceDayOff();

        /// <summary>
        /// Check preference Shift
        /// </summary>
        /// <returns></returns>
        PermissionState CheckPreferenceShift();

        /// <summary>
        /// Check rotation Day Off
        /// </summary>
        /// <returns></returns>
        PermissionState CheckRotationDayOff();

        /// <summary>
        /// Check rotation Shift
        /// </summary>
        /// <returns></returns>
        PermissionState CheckRotationShift();

        /// <summary>
        /// Get schedule day
        /// </summary>
        IScheduleDay ScheduleDay { get; set; }
    }
}