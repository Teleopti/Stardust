using System;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Holds the user defined preferences for ResourceOptimizer operation
    /// </summary>
    public interface IOptimizerAdvancedPreferences : ICloneable
    {

        /// <summary>
        /// Gets or sets a value indicating whether the move time operation is allowed.
        /// </summary>
        /// <value><c>true</c> if allow; otherwise, <c>false</c>.</value>
        bool AllowWorkShiftOptimization { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [allow extend reduce time optimization].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [allow extend reduce time optimization]; otherwise, <c>false</c>.
        /// </value>
        bool AllowExtendReduceTimeOptimization { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the day off optimization is allowed (whether the user can move day offs).
        /// </summary>
        /// <value>
        /// 	<c>true</c> if allow; otherwise, <c>false</c>.
        /// </value>
        bool AllowDayOffOptimization { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the intraday optimization is allowed.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [allow intraday optimization]; otherwise, <c>false</c>.
        /// </value>
        bool AllowIntradayOptimization { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of movable day-offs per person.
        /// </summary>
        /// <value>The maximum number of movable day-offs per person.</value>
        double MaximumMovableDayOffPercentagePerPerson { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of movable workdays per person.
        /// </summary>
        /// <value>The maximum number of movable workdays per person.</value>
        double MaximumMovableWorkShiftPercentagePerPerson { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to use maximum standard deviation in the optimization.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if use maximum standard deviation; otherwise, <c>false</c>.
        /// </value>
        bool ConsiderMaximumIntraIntervalStandardDeviation { get; set; }

        /// <summary>
        /// Gets or sets value indicating whether to use old or improved calculating
        /// </summary>
        bool UseTweakedValues { get; set; }


        /// <summary>
        /// Gets or sets a value indicating whether to use standard deviation in the calculation method.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if use standard deviation; <c>false</c> if user RMS.
        /// </value>
        bool UseStandardDeviationCalculation { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [use teleopti calculation].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [use teleopti calculation]; otherwise, <c>false</c>.
        /// </value>
        bool UseTeleoptiCalculation { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [allow extend reduce daysoff optimization].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [allow extend reduce daysoff optimization]; otherwise, <c>false</c>.
        /// </value>
        bool AllowExtendReduceDaysOffOptimization { get; set; }
    }
}