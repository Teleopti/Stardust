namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Defines what restrictions to use when validating
    /// </summary>
    public interface ILegalStateCheckerOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether [use rotation].
        /// </summary>
        /// <value><c>true</c> if [use rotation]; otherwise, <c>false</c>.</value>
        bool UseRotations
        { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [use availability].
        /// </summary>
        /// <value><c>true</c> if [use availability]; otherwise, <c>false</c>.</value>
        bool UseAvailability
        { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [use student].
        /// </summary>
        /// <value><c>true</c> if [use student]; otherwise, <c>false</c>.</value>
        bool UseStudentAvailability
        { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [use preference].
        /// </summary>
        /// <value><c>true</c> if [use preference]; otherwise, <c>false</c>.</value>
        bool UsePreferences
        { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [use schedule].
        /// </summary>
        /// <value><c>true</c> if [use schedule]; otherwise, <c>false</c>.</value>
        bool UseSchedule
        { get; set; }
    }
}