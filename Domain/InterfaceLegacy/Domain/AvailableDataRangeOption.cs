namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Available data range enumeration
    /// </summary>
    public enum AvailableDataRangeOption
    {
        /// <summary>
        /// No available data 
        /// </summary>
        None,
        /// <summary>
        /// Person's own data
        /// </summary>
        MyOwn,
        /// <summary>
        /// Person's Team data
        /// </summary>
        MyTeam,
        /// <summary>
        /// Person's Site data
        /// </summary>
        MySite,
        /// <summary>
        /// Person's business unit data
        /// </summary>
        MyBusinessUnit,
        /// <summary>
        /// Everyone in the system. Exclusively for the super user
        /// </summary>
        Everyone
    }
}
