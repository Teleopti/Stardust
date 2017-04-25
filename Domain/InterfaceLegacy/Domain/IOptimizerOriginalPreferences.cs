using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    /// <summary>
    /// Contains the user defined original preferences
    /// </summary>
    public interface IOptimizerOriginalPreferences
    {

        /// <summary>
        /// Gets the scheduling options.
        /// </summary>
        SchedulingOptions SchedulingOptions { get; }
    }
}