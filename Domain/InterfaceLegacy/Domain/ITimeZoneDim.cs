namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Handles time zones for ETL Tool
    /// </summary>
    public interface ITimeZoneDim
    {
        /// <summary>
        /// Gets the data mart id for the time zone.
        /// </summary>
        /// <value>The time zone data mart id.</value>
        int MartId { get; }

        /// <summary>
        /// Gets the time zone code. Equals to 'TimeZoneInfo.Id' if exist, else default -1.
        /// </summary>
        /// <value>The time zone code.</value>
        string TimeZoneCode { get; }

        /// <summary>
        /// Gets the name of the time zone. Equals 'TimeZoneInfo.DisplayName'.
        /// </summary>
        /// <value>The name of the time zone.</value>
        string TimeZoneName { get; }

        /// <summary>
        /// Gets a value indicating whether this time zone is the default time zone used by the ETL Tool.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is default time zone; otherwise, <c>false</c>.
        /// </value>
        bool IsDefaultTimeZone { get; }

        /// <summary>
        /// Gets the UTC conversion in minutes.
        /// </summary>
        /// <value>The UTC conversion.</value>
        int UtcConversion { get; }

        /// <summary>
        /// Gets the UTC conversion DST (DayligtSavings Time) in minutes.
        /// </summary>
        /// <value>The UTC conversion DST.</value>
        int UtcConversionDst { get; }
    }
}