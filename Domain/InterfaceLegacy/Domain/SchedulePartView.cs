
namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// What is most significant when doing a "day projection"
    /// </summary>
    public enum SchedulePartView
    {
        /// <summary>
        /// An absence covering all schedule part
        /// </summary>
        FullDayAbsence,
        /// <summary>
        /// Main shift
        /// </summary>
        MainShift,
        /// <summary>
        /// Day off
        /// </summary>
        DayOff,
        /// <summary>
        /// Personal shift
        /// </summary>
        PersonalShift,
        /// <summary>
        /// Absence
        /// </summary>
        Absence,
        /// <summary>
        /// Nothing
        /// </summary>
        None,
        /// <summary>
        /// Preference restriction
        /// </summary>
        PreferenceRestriction,
        /// <summary>
        /// StudentAvailabilityRestriction
        /// </summary>
        StudentAvailabilityRestriction,
        /// <summary>
        /// Overtime
        /// </summary>
        Overtime,

        ///<summary>
        /// Day Off from Contract Schedule
        ///</summary>
        ContractDayOff
    }
}
