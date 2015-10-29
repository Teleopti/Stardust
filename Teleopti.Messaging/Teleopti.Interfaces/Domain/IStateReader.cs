using System;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Interface for reading different kinds of state
    /// </summary>
    public interface IStateReader
    {
        /// <summary>
        /// Gets a value indicating whether the person is logged in or not.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if logged in; otherwise, <c>false</c>.
        /// </value>
        bool IsLoggedIn { get; }

		TimeZoneInfo UserTimeZone { get; }

		/// <summary>
		/// Gets the application scope data.
		/// </summary>
		/// <value>The application scope data.</value>
		IApplicationData ApplicationScopeData { get; }
    }
}