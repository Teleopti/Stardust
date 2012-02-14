using Teleopti.Interfaces.Domain;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Contains the preferences, bot user defined and actual to the resource reoptimizer process
    /// </summary>
    public interface IOptimizerPreferences
    {
        /// <summary>
        /// Gets or sets the user defined preferences.
        /// </summary>
        /// <value>The user defined preferences.</value>
        IOptimizerOriginalPreferences OriginalOptimizerPreferences { get; }

        /// <summary>
        /// Gets or sets the current iteration pereferences.
        /// </summary>
        /// <value>The current operation.</value>
        IOptimizerIterationPreferences IterationOptimizerPreferences { get;}
        
    }
}