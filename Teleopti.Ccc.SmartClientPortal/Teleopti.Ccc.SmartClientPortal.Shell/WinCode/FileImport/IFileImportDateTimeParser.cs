using System;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.FileImport
{
    /// <summary>
    /// 
    /// </summary>
    public interface IFileImportDateTimeParser
    {

        /// <summary>
        /// Returns a UTC DateTime
        /// </summary>
        /// <param name="dateValue">The date value.</param>
        /// <param name="timeValue">The time value.</param>
        /// <returns></returns>
        /// <remarks>
        /// The specified format is HH:MM for timeString
        /// The specified format is YYYYMMDD for dateString
        /// This is the decided format for import?
        /// </remarks>
        DateTime UtcDateTime(string dateValue, string timeValue);


        /// <summary>
        /// Returns a string in FileImportDo specified format
        /// </summary>
        /// <param name="dateValue">The date value.</param>
        /// <param name="timeValue">The time value.</param>
        /// <returns>Time of day in utc in format HH:MM</returns>
        /// <remarks>
        /// The specified format is HH:MM for timeString
        /// The specified format is YYYYMMDD for dateString
        /// This is the decided format for import?
        /// </remarks>
        string UtcTime(string dateValue, string timeValue);

        /// <summary>
        /// Gets or sets the time zone for parsing
        /// </summary>
        /// <param name="timeZoneForConverting">The time zone for converting.</param>
        /// <value>The time zone.</value>
        void TimeZone(TimeZoneInfo timeZoneForConverting);

        /// <summary>
        /// Dates the time is valid.
        /// </summary>
        /// <param name="dateValue">The date value.</param>
        /// <param name="timeValue">The time value.</param>
        /// <returns></returns>
        bool DateTimeIsValid(string dateValue, string timeValue);
    }

}
