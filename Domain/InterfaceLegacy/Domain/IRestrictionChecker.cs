
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
        PermissionState CheckAvailability(IScheduleDay scheduleDay);

        /// <summary>
        /// Check rotations
        /// </summary>
        /// <returns></returns>
		PermissionState CheckRotations(IScheduleDay scheduleDay);

        /// <summary>
        /// Check student availability
        /// </summary>
        /// <returns></returns>
		PermissionState CheckStudentAvailability(IScheduleDay scheduleDay);

        /// <summary>
        /// Check preference
        /// </summary>
        /// <returns></returns>
		PermissionState CheckPreference(IScheduleDay scheduleDay);

        /// <summary>
        /// Check preference must have
        /// </summary>
        /// <returns></returns>
		PermissionState CheckPreferenceMustHave(IScheduleDay scheduleDay);

        /// <summary>
		/// Check preference absence
        /// </summary>
        /// <param name="permissionState"></param>
        /// <param name="schedulePart"></param>
        /// <returns></returns>
		PermissionState CheckPreferenceAbsence(PermissionState permissionState, IScheduleDay schedulePart);

        /// <summary>
        /// Check preference Day Off
        /// </summary>
        /// <returns></returns>
		PermissionState CheckPreferenceDayOff(IScheduleDay schedulePart);

        /// <summary>
        /// Check preference Shift
        /// </summary>
        /// <returns></returns>
		PermissionState CheckPreferenceShift(IScheduleDay schedulePart);

        /// <summary>
        /// Check rotation Day Off
        /// </summary>
        /// <returns></returns>
		PermissionState CheckRotationDayOff(IScheduleDay schedulePart);

        /// <summary>
        /// Check rotation Shift
        /// </summary>
        /// <returns></returns>
		PermissionState CheckRotationShift(IScheduleDay schedulePart);
    }
}