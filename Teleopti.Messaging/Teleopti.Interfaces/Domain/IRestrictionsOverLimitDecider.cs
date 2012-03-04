namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Restrictions over the limit decider
    /// </summary>
    public interface IRestrictionsOverLimitDecider
    {
        /// <summary>
        /// Check if preferenceses are over the limit.
        /// </summary>
        /// <returns></returns>
        bool PreferencesOverLimit();

        /// <summary>
        /// Check if must haves are over the limit.
        /// </summary>
        /// <returns></returns>
        bool MustHavesOverLimit();

        /// <summary>
        /// Check if rotations are over the limit.
        /// </summary>
        /// <returns></returns>
        bool RotationOverLimit();

        /// <summary>
        /// Check if preferenceses are over the limit.
        /// </summary>
        /// <returns></returns>
        bool AvailabilitiesOverLimit();

        /// <summary>
        /// Check if preferenceses are over the limit.
        /// </summary>
        /// <returns></returns>
        bool StudentAvailabilitiesOverLimit();
    }
}