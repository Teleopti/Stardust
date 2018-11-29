namespace Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure
{
    /// <summary>
    /// Represents a set of passwordstrength-rules
    /// </summary>
    /// <remarks>
    /// Created by: henrika
    /// Created date: 2010-08-17
    /// </remarks>
    public interface IPasswordStrengthRule
    {

        /// <summary>
        /// Verifies the password strength.
        /// </summary>
        /// <param name="password">The password.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2010-08-18
        /// </remarks>
        bool VerifyPasswordStrength(string password);
    }

}
