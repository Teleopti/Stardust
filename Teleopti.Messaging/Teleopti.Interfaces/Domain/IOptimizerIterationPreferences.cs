using Teleopti.Interfaces.Domain;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Contains the preferences that is actual to the current resource reoptimizer iteration only.
    /// Note that the actual value can change on the way of the optimization process.
    /// </summary>
    public interface IOptimizerIterationPreferences
    {

        /// <summary>
        /// Gets or sets the iteration criteria preference.
        /// </summary>
        /// <value>The iteration criteria preference.</value>
        IOptimizerAdvancedPreferences IterationAdvancedPreferences { get; set; }

        /// <summary>
        /// Gets or sets the day-off planner rules that are valid for the current iteration..
        /// </summary>
        /// <value>The iteration day off planner rules.</value>
        IDayOffPlannerRules IterationDayOffPlannerRules { get; set; }

        /// <summary>
        /// Gets or sets the current operation.
        /// </summary>
        /// <value>The current operation.</value>
        IterationOperationOption IterationOperation { get; set; }

        /// <summary>
        /// Gets the iteration scheduling options.
        /// </summary>
        /// <value>The iteration scheduling options.</value>
        ISchedulingOptions IterationSchedulingOptions { get; set; }
        
    }
}