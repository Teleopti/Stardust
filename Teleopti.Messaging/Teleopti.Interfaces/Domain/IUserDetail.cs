using System;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// Created by: peterwe
    /// Created date: 2010-08-19
    /// </remarks>
    public interface IUserDetail : IAggregateRoot
    {
        /// <summary>
        /// Gets the last password change.
        /// </summary>
        /// <value>The last password change.</value>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2010-08-19
        /// </remarks>
        DateTime LastPasswordChange { get; }
        /// <summary>
        /// Gets the invalid attempts.
        /// </summary>
        /// <value>The invalid attempts.</value>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2010-08-19
        /// </remarks>
        int InvalidAttempts { get; }
        /// <summary>
        /// Gets a value indicating whether this instance is locked.
        /// </summary>
        /// <value><c>true</c> if this instance is locked; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2010-08-19
        /// </remarks>
        bool IsLocked { get; }
        /// <summary>
        /// Gets the invalid attempts sequence start.
        /// </summary>
        /// <value>The invalid attempts sequence start.</value>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2010-08-19
        /// </remarks>
        DateTime InvalidAttemptsSequenceStart { get; }
        /// <summary>
        /// Gets the person.
        /// </summary>
        /// <value>The person.</value>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2010-08-19
        /// </remarks>
        IPerson Person { get; }
        /// <summary>
        /// Locks this instance.
        /// </summary>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2010-08-19
        /// </remarks>
        void Lock();
        /// <summary>
        /// Registers the invalid attempt.
        /// </summary>
        /// <param name="passwordPolicy">The password policy.</param>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2010-08-19
        /// </remarks>
        void RegisterInvalidAttempt(IPasswordPolicy passwordPolicy);
        /// <summary>
        /// Registers the password change.
        /// </summary>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2010-08-19
        /// </remarks>
        void RegisterPasswordChange();
    }
}