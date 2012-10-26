using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Interfaces.Infrastructure
{
    /// <summary>
    /// Data for logged on Person
    /// </summary>
    public interface ISessionData
    {
        /// <summary>
        /// Gets or sets the time zone.
        /// </summary>
        /// <value>The time zone.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-10-22
        /// </remarks>
        TimeZoneInfo TimeZone { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether Micke is in the mode or not.
        /// </summary>
        /// <value><c>true</c> if [micke mode]; otherwise, <c>false</c>.</value>
        bool MickeMode { get; set; }

        /// <summary>
        /// Gets or sets the clip. Is used to transport referenses between modules within this instance
        /// A sort of internal ClipBoard
        /// </summary>
        /// <value>The clip.</value>
        object Clip { get; set; }

        ///<summary>
        /// The authentication type used when logging on to the system.
        ///</summary>
        AuthenticationTypeOption AuthenticationTypeOption { get; set; }
    }
}