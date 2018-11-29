using System.Collections.Generic;
using System.ComponentModel;
using Teleopti.Ccc.Domain.Optimization;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    /// <summary>
    /// Optimization user defined preferences
    /// </summary>
    public interface IOptimizationPreferences
    {
        /// <summary>
        /// Gets or sets the general user preferences.
        /// </summary>
        /// <value>The general.</value>
        GeneralPreferences General { get; set; }

        /// <summary>
        /// Gets or sets the extra user preferences.
        /// </summary>
        /// <value>The extra.</value>
        ExtraPreferences Extra { get; set; }

        /// <summary>
        /// Gets or sets the advanced user preferences.
        /// </summary>
        /// <value>The advanced.</value>
        IAdvancedPreferences Advanced { get; set; }

        /// <summary>
        /// Gets or sets the local scheduling options.
        /// </summary>
        /// <value>The local scheduling options.</value>
        IReschedulingPreferences Rescheduling { get; set; }

        ShiftPreferences Shifts { get; set; }

	    bool ShiftBagBackToLegal { get; set; }
    }

    /// <summary>
    /// Day Off user optimization preferences
    /// </summary>
    public interface IDaysOffPreferences
    {
        /// <summary>
        /// Gets or sets a value indicating whether to use keep existing days off.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if use; otherwise, <c>false</c>.
        /// </value>
        bool UseKeepExistingDaysOff { get; set; }

        /// <summary>
        /// Gets or sets the keep existing days off value.
        /// </summary>
        /// <value>The keep existing days off value.</value>
        double KeepExistingDaysOffValue { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether yo use days off per week.
        /// </summary>
        /// <value><c>true</c> if use; otherwise, <c>false</c>.</value>
        bool UseDaysOffPerWeek { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to use consecutive days off.
        /// </summary>
        /// <value><c>true</c> if use; otherwise, <c>false</c>.</value>
        bool UseConsecutiveDaysOff { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to use consecutive workdays.
        /// </summary>
        /// <value><c>true</c> if use; otherwise, <c>false</c>.</value>
        bool UseConsecutiveWorkdays { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to use full weekends off.
        /// </summary>
        /// <value><c>true</c> if use; otherwise, <c>false</c>.</value>
        bool UseFullWeekendsOff { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to use week end days off.
        /// </summary>
        /// <value><c>true</c> if use; otherwise, <c>false</c>.</value>
        bool UseWeekEndDaysOff { get; set; }

        /// <summary>
        /// Gets or sets the days off per week value.
        /// </summary>
        /// <value>The days off per week value.</value>
        MinMax<int> DaysOffPerWeekValue { get; set; }

        /// <summary>
        /// Gets or sets the consecutive days off value.
        /// </summary>
        /// <value>The consecutive days off value.</value>
        MinMax<int> ConsecutiveDaysOffValue { get; set; }

        /// <summary>
        /// Gets or sets the consecutive workdays value.
        /// </summary>
        /// <value>The consecutive workdays value.</value>
        MinMax<int> ConsecutiveWorkdaysValue { get; set; }

        /// <summary>
        /// Gets or sets the full weekends off value.
        /// </summary>
        /// <value>The full weekends off value.</value>
        MinMax<int> FullWeekendsOffValue { get; set; }

        /// <summary>
        /// Gets or sets the week end days off value.
        /// </summary>
        /// <value>The week end days off value.</value>
        MinMax<int> WeekEndDaysOffValue { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to consider the week before.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if consider; otherwise, <c>false</c>.
        /// </value>
        bool ConsiderWeekBefore { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to consider the week after.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if consider; otherwise, <c>false</c>.
        /// </value>
        bool ConsiderWeekAfter { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to keep free weekends.
        /// </summary>
        /// <value><c>true</c> if keep free weekends; otherwise, <c>false</c>.</value>
        bool KeepFreeWeekends { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to keep free weekend days.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if keep free weekend days; otherwise, <c>false</c>.
        /// </value>
        bool KeepFreeWeekendDays { get; set; }
    }

    /// <summary>
    /// Advanced user preferences
    /// </summary>
    public interface IAdvancedPreferences
	{
        /// <summary>
        /// Gets or sets the target value.
        /// </summary>
        /// <value>The target value.</value>
        TargetValueOptions TargetValueCalculation { get; set; }        

        /// <summary>
        /// Gets or sets a value indicating whether [use tweaked values].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if use; otherwise, <c>false</c>.
        /// </value>
        bool UseTweakedValues { get; set; }

		  /// <summary>
		  /// Gets or sets a value indicating whether [use maximum seats].
		  /// </summary>
		  /// <value>
		  /// 	<c>true</c> if use; otherwise, <c>false</c>.
		  /// </value>
		  MaxSeatsFeatureOptions UserOptionMaxSeatsFeature { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether [use average shift lengths].
		/// </summary>
		/// <value>
		/// 	<c>true</c> if [use average shift lengths]; otherwise, <c>false</c>.
		/// </value>
		bool UseAverageShiftLengths { get; set; }

        /// <summary>
        /// Gets or sets the refresh screen interval.
        /// </summary>
        /// <value>The refresh screen interval.</value>
        int RefreshScreenInterval { get; set; }

		bool UseMinimumStaffing { get; set; }
		bool UseMaximumStaffing { get; set; }
	}

    /// <summary>
    /// Target value calculation options
    /// </summary>
    public enum TargetValueOptions
    {
        /// <summary>
        /// Standard deviation
        /// </summary>
        StandardDeviation,

        /// <summary>
        /// Root Mean Square
        /// </summary>
        RootMeanSquare,

        /// <summary>
        /// Teleopti special
        /// </summary>
        Teleopti
    }

    /// <summary>
    /// Contains local information to the scheduling phase in optimization
    /// </summary>
    public interface IReschedulingPreferences
    {
                /// <summary>
        /// Gets or sets a value indicating whether to consider short breaks.
        /// </summary>
        /// <value><c>true</c> if consider short breaks; otherwise, <c>false</c>.</value>
        [DefaultValue(true)]
        bool ConsiderShortBreaks { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to use only shifts when understaffed.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if use only shifts when understaffed; otherwise, <c>false</c>.
        /// </value>
        bool OnlyShiftsWhenUnderstaffed { get; set; }

	    IDictionary<IPerson, IScheduleRange> AllSelectedScheduleRangeClones { get; set; }
    }

}
