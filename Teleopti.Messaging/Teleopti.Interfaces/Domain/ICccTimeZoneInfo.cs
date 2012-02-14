using System;
using System.Runtime.Serialization;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Raptor Time Zone Info
    /// </summary>
    /// <remarks>
    /// Created by: ankarlp
    /// Created date: 2008-08-19
    /// </remarks>
    public interface ICccTimeZoneInfo : IEquatable<ICccTimeZoneInfo>, ISerializable
    {
        // Not used anywhere, Henry 2008-11-03
        ///// <summary>
        ///// Gets the ambiguous time offsets.
        ///// </summary>
        ///// <param name="dateTime">The date time.</param>
        ///// <returns></returns>
        ///// <remarks>
        ///// Created by: ankarlp
        ///// Created date: 2008-08-19
        ///// </remarks>
        //TimeSpan[] GetAmbiguousTimeOffsets(DateTime dateTime);
        ///// <summary>
        ///// Gets the ambiguous time offsets.
        ///// </summary>
        ///// <param name="dateTimeOffset">The date time offset.</param>
        ///// <returns></returns>
        ///// <remarks>
        ///// Created by: ankarlp
        ///// Created date: 2008-08-19
        ///// </remarks>
        //TimeSpan[] GetAmbiguousTimeOffsets(DateTimeOffset dateTimeOffset);

        /// <summary>
        /// Gets the UTC offset.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 2008-08-19
        /// </remarks>
        TimeSpan GetUtcOffset(DateTime dateTime);
        /// <summary>
        /// Gets the UTC offset.
        /// </summary>
        /// <param name="dateTimeOffset">The date time offset.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 2008-08-19
        /// </remarks>
        TimeSpan GetUtcOffset(DateTimeOffset dateTimeOffset);
        /// <summary>
        /// Determines whether [is ambiguous time] [the specified date time].
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <returns>
        /// 	<c>true</c> if [is ambiguous time] [the specified date time]; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 2008-08-19
        /// </remarks>
        bool IsAmbiguousTime(DateTime dateTime);
        /// <summary>
        /// Determines whether [is ambiguous time] [the specified date time offset].
        /// </summary>
        /// <param name="dateTimeOffset">The date time offset.</param>
        /// <returns>
        /// 	<c>true</c> if [is ambiguous time] [the specified date time offset]; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 2008-08-19
        /// </remarks>
        bool IsAmbiguousTime(DateTimeOffset dateTimeOffset);
        /// <summary>
        /// Determines whether [is daylight saving time] [the specified date time].
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <returns>
        /// 	<c>true</c> if [is daylight saving time] [the specified date time]; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 2008-08-19
        /// </remarks>
        bool IsDaylightSavingTime(DateTime dateTime);
        /// <summary>
        /// Determines whether [is daylight saving time] [the specified date time offset].
        /// </summary>
        /// <param name="dateTimeOffset">The date time offset.</param>
        /// <returns>
        /// 	<c>true</c> if [is daylight saving time] [the specified date time offset]; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 2008-08-19
        /// </remarks>
        bool IsDaylightSavingTime(DateTimeOffset dateTimeOffset);
        /// <summary>
        /// Determines whether [is invalid time] [the specified date time].
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <returns>
        /// 	<c>true</c> if [is invalid time] [the specified date time]; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 2008-08-19
        /// </remarks>
        bool IsInvalidTime(DateTime dateTime);
        /// <summary>
        /// To serialized string.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 2008-08-19
        /// </remarks>
        string ToSerializedString();
        /// <summary>
        /// To string.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 2008-08-19
        /// </remarks>
        string ToString();
        /// <summary>
        /// Gets the base UTC offset.
        /// </summary>
        /// <value>The base UTC offset.</value>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 2008-08-19
        /// </remarks>
        TimeSpan BaseUtcOffset { get; }
        /// <summary>
        /// Gets the name of the daylight.
        /// </summary>
        /// <value>The name of the daylight.</value>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 2008-08-19
        /// </remarks>
        string DaylightName { get; }
        /// <summary>
        /// Gets the display name.
        /// </summary>
        /// <value>The display name.</value>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 2008-08-19
        /// </remarks>
        string DisplayName { get; }
        /// <summary>
        /// Gets the id.
        /// </summary>
        /// <value>The id.</value>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 2008-08-19
        /// </remarks>
        string Id { get; }
        /// <summary>
        /// Gets the name of the standard.
        /// </summary>
        /// <value>The name of the standard.</value>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 2008-08-19
        /// </remarks>
        string StandardName { get; }
        /// <summary>
        /// Gets a value indicating whether [supports daylight saving time].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [supports daylight saving time]; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 2008-08-19
        /// </remarks>
        bool SupportsDaylightSavingTime { get; }

        /// <summary>
        /// Gets the time zone info object.
        /// </summary>
        /// <value>The time zone info object.</value>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 2008-08-19
        /// </remarks>
        Object TimeZoneInfoObject { get; }

        /// <summary>
        /// Gets the UTC.
        /// </summary>
        /// <value>The UTC.</value>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 2008-08-19
        /// </remarks>
        ICccTimeZoneInfo Utc { get; }

        /// <summary>
        /// Converts the time from UTC.
        /// </summary>
        /// <param name="startDateTime">The start date time.</param>
        /// <param name="timeZoneInfo">The time zone info.</param>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 2008-08-19
        /// </remarks>
        DateTime ConvertTimeFromUtc(DateTime startDateTime, ICccTimeZoneInfo timeZoneInfo);

        /// <summary>
        /// Converts the time to UTC.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 2008-08-19
        /// </remarks>
        DateTime ConvertTimeToUtc(DateTime dateTime);

        /// <summary>
        /// Converts the time to UTC.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <param name="timeZoneInfo"></param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 2008-08-19
        /// </remarks>
        DateTime ConvertTimeToUtc(DateTime dateTime, ICccTimeZoneInfo timeZoneInfo);

        /// <summary>
        /// Converts the time from UTC.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2010-05-18
        /// </remarks>
        DateTime ConvertTimeFromUtc(DateTime dateTime);
    }
}
