using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Time
{
    /// <summary>
    /// Used for setting a default period based on inputs
    /// </summary>
    /// <remarks>
    /// Created by: henrika
    /// Created date: 2010-09-07
    /// </remarks>
    public interface ISetupDateTimePeriod
    {
        /// <summary>
        /// Gets the period.
        /// </summary>
        /// <value>The period.</value>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2010-09-07
        /// </remarks>
        DateTimePeriod Period { get; }
    }
}