using System;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Responsible for handling rules for password and login
    /// </summary>
    /// <remarks>
    /// Created by: henrika
    /// Created date: 2010-08-17
    /// </remarks>
    public interface IPasswordPolicy
    {
        /// <summary>
        /// Gets the invalid attempt window.
        /// </summary>
        /// <value>The invalid attempt window.</value>
        TimeSpan InvalidAttemptWindow { get; }

        /// <summary>
        /// Gets the max attempt count.
        /// </summary>
        /// <value>The max attempt count.</value>
        int MaxAttemptCount { get; }

        /// <summary>
        /// Checks the password strength.
        /// </summary>
        /// <param name="password">The password.</param>
        /// <returns>true if the password passes all the rules</returns>
        bool CheckPasswordStrength(string password);

        ///<summary>
        /// Number of days a password is valid for
        ///</summary>
        int PasswordValidForDayCount { get; }

        ///<summary>
        /// Number of days before the password will expire that a warning should be displayed
        ///</summary>
        int PasswordExpireWarningDayCount { get; }

    }
}