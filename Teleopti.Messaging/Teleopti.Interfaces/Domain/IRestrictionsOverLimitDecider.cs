using System.Collections.Generic;

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
        IList<DateOnly> PreferencesOverLimit();

        /// <summary>
        /// Check if must haves are over the limit.
        /// </summary>
        /// <returns></returns>
        IList<DateOnly> MustHavesOverLimit();

        /// <summary>
        /// Check if rotations are over the limit.
        /// </summary>
        /// <returns></returns>
        IList<DateOnly> RotationOverLimit();

        /// <summary>
        /// Check if preferenceses are over the limit.
        /// </summary>
        /// <returns></returns>
        IList<DateOnly> AvailabilitiesOverLimit();

        /// <summary>
        /// Check if preferenceses are over the limit.
        /// </summary>
        /// <returns></returns>
        IList<DateOnly> StudentAvailabilitiesOverLimit();
    }
}