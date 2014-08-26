using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Interfaces.Infrastructure
{
    /// <summary>
    /// Responsible for getting the PasswordPolicies
    /// </summary>
    /// <remarks>
    /// Created by: henrika
    /// Created date: 2010-08-17
    /// </remarks>
    public interface ILoadPasswordPolicyService
    {
        /// <summary>
        /// Gets the invalid attempt window
        /// (the minimum timespan allowed for MaxAttemptCount)
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2010-08-17
        /// </remarks>
        TimeSpan LoadInvalidAttemptWindow();

        /// <summary>
        /// Gets the max attempt count.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2010-08-17
        /// </remarks>
        int LoadMaxAttemptCount();



        ///<summary>
        /// Loads the number of days a password is valid for
        ///</summary>
        int LoadPasswordValidForDayCount();

        ///<summary>
        /// Loads the number of days before the password will expire that a warning should be displayed
        ///</summary>
        int LoadPasswordExpireWarningDayCount();


        /// <summary>
        /// Gets all the password strength rules.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2010-08-17
        /// </remarks>
        IList<IPasswordStrengthRule> LoadPasswordStrengthRules();

        /// <summary>
        /// Gets or sets the path.
        /// </summary>
        /// <value>The path.</value>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2010-08-19
        /// </remarks>
        string Path { get; set; }

	    void ClearFile();
    }
}
