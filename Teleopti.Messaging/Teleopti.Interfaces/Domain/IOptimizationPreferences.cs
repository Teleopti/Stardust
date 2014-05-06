using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Teleopti.Interfaces.Domain
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
        IGeneralPreferences General { get; set; }

        /// <summary>
        /// Gets or sets the days off user preferences.
        /// </summary>
        /// <value>The days off.</value>
        IDaysOffPreferences DaysOff { get; set; }

        /// <summary>
        /// Gets or sets the extra user preferences.
        /// </summary>
        /// <value>The extra.</value>
        IExtraPreferences Extra { get; set; }

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

        /// <summary>
        /// 
        /// </summary>
        IShiftPreferences Shifts { get; set; }
    }

    /// <summary>
    /// Optimization user defined general preferences
    /// </summary>>
    public interface IGeneralPreferences
    {
        /// <summary>
        /// Gets or sets the schedule tag.
        /// </summary>
        /// <value>The schedule tag.</value>
        IScheduleTag ScheduleTag { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to use days off optimization step.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if use step; otherwise, <c>false</c>.
        /// </value>
        bool OptimizationStepDaysOff { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to move time between days step.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if use step; otherwise, <c>false</c>.
        /// </value>
        bool OptimizationStepTimeBetweenDays { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to use shifts for flexible work time step.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if use step; otherwise, <c>false</c>.
        /// </value>
        bool OptimizationStepShiftsForFlexibleWorkTime { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to use days off for flexible work time step.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if use step; otherwise, <c>false</c>.
        /// </value>
        bool OptimizationStepDaysOffForFlexibleWorkTime { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether to run the fairness step.
		/// </summary>
		bool OptimizationStepFairness { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether  to use time between days optimization step.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if use step; otherwise, <c>false</c>.
        /// </value>
        bool OptimizationStepShiftsWithinDay { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to use preferences.
        /// </summary>
        /// <value><c>true</c> if use preferences; otherwise, <c>false</c>.</value>
        bool UsePreferences { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to use must haves.
        /// </summary>
        /// <value><c>true</c> if use must haves; otherwise, <c>false</c>.</value>
        bool UseMustHaves { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to use rotations.
        /// </summary>
        /// <value><c>true</c> if use rotations; otherwise, <c>false</c>.</value>
        bool UseRotations { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to use availabilities.
        /// </summary>
        /// <value><c>true</c> if use availabilities; otherwise, <c>false</c>.</value>
        bool UseAvailabilities { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to use student availabilities.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if use student availabilities; otherwise, <c>false</c>.
        /// </value>
        bool UseStudentAvailabilities { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to use shift category limitations.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if use shift category limitations; otherwise, <c>false</c>.
        /// </value>
        bool UseShiftCategoryLimitations { get; set; }

        /// <summary>
        /// Gets or sets the preferences value.
        /// </summary>
        /// <value>The preferences value.</value>
        double PreferencesValue { get; set; }

        /// <summary>
        /// Gets or sets the must haves value.
        /// </summary>
        /// <value>The must haves value.</value>
        double MustHavesValue { get; set; }

        /// <summary>
        /// Gets or sets the rotations value.
        /// </summary>
        /// <value>The rotations value.</value>
        double RotationsValue { get; set; }

        /// <summary>
        /// Gets or sets the availabilities value.
        /// </summary>
        /// <value>The availabilities value.</value>
        double AvailabilitiesValue { get; set; }

        /// <summary>
        /// Gets or sets the student availabilities value.
        /// </summary>
        /// <value>The student availabilities value.</value>
        double StudentAvailabilitiesValue { get; set; }
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
    /// Extra optimization preferences
    /// </summary>
    public interface IExtraPreferences
    {
        ///// <summary>
        ///// Gets or sets a value indicating whether to use block scheduling.
        ///// </summary>
        ///// <value><c>true</c> if use block; otherwise, <c>false</c>.</value>
        //bool UseBlockScheduling { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to use team scheduling.
        /// </summary>
        /// <value><c>true</c> if use team scheduling; otherwise, <c>false</c>.</value>
        bool UseTeams { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether [keep same days off in team].
		/// </summary>
		/// <value>
		/// 	<c>true</c> if [keep same days off in team]; otherwise, <c>false</c>.
		/// </value>
		bool UseTeamSameDaysOff { get; set; }

        /// <summary>
        /// Gets or sets the group page on team.
        /// </summary>
        /// <value>The group page on team.</value>
        IGroupPageLight TeamGroupPage { get; set; }

        /// <summary>
        /// Gets or sets the fairness value.
        /// </summary>
        /// <value>The fairness value.</value>
        double FairnessValue { get; set; }

        /// <summary>
        /// Gets or sets the group page on compare with.
        /// </summary>
        /// <value>The group page on compare with.</value>
        IGroupPageLight GroupPageOnCompareWith { get; set; }

		/// <summary>
		/// 
		/// </summary>
		bool UseTeamSameStartTime { get; set; }
		/// <summary>
		/// 
		/// </summary>
		bool UseTeamSameEndTime { get; set; }
		/// <summary>
		/// 
		/// </summary>
		bool UseTeamSameShiftCategory { get; set; }
        /// <summary>
        /// Use Common Activity
        /// </summary>
        bool UseTeamSameActivity { get; set; }
        /// <summary>
        /// The actual common activity
        /// </summary>
        IActivity TeamActivityValue { get; set; }

        /// <summary>
        /// Block finder service for advance optimization service
        /// </summary>
        BlockFinderType BlockTypeValue { get; set; }

        /// <summary>
        /// Use TeamBlock same end time
        /// </summary>
        bool UseBlockSameEndTime { get; set; }
        /// <summary>
        /// Use TeamBlock same shift category
        /// </summary>
        bool UseBlockSameShiftCategory { get; set; }
        /// <summary>
        /// Use TeamBlock same start time
        /// </summary>
        bool UseBlockSameStartTime { get; set; }
        /// <summary>
        /// Use TeamBlock same shift
        /// </summary>
        bool UseBlockSameShift { get; set; }
        /// <summary>
        /// This is  used if TeamBlock per is used
        /// </summary>
        bool UseTeamBlockOption { get; set; }
    }

    /// <summary>
    /// Extra optimization preferences
    /// </summary>
    public interface IShiftPreferences
    {
        /// <summary>
        /// Gets or sets a value indicating whether to keep shift categories.
        /// </summary>
        /// <value><c>true</c> if [keep shift categories]; otherwise, <c>false</c>.</value>
        bool KeepShiftCategories { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to keep start and end times.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if keep start and end times; otherwise, <c>false</c>.
        /// </value>
        bool KeepStartTimes { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to keep start and end times.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if keep start and end times; otherwise, <c>false</c>.
        /// </value>
        bool KeepEndTimes { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to keep shifts.
        /// </summary>
        /// <value><c>true</c> if keep shifts; otherwise, <c>false</c>.</value>
        bool KeepShifts { get; set; }

        /// <summary>
        /// Gets or sets the keep shifts value.
        /// </summary>
        /// <value>The keep shifts value.</value>
        double KeepShiftsValue { get; set; }

        /// <summary>
        /// Alter between property
        /// </summary>
        bool AlterBetween { get; set; }

        /// <summary>
        /// The selected Guids
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        IList<IActivity > SelectedActivities { get; set; }

        /// <summary>
        /// 
        /// </summary>
        TimePeriod SelectedTimePeriod { get; set; }

	    bool KeepActivityLength { get; set; }
	    IActivity ActivityToKeepLengthOn { get; set; }
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
        /// Gets or sets a value indicating whether to use intra interval deviation.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if use; otherwise, <c>false</c>.
        /// </value>
        bool UseIntraIntervalDeviation { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [use tweaked values].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if use; otherwise, <c>false</c>.
        /// </value>
        bool UseTweakedValues { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [use minimum staffing].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if use; otherwise, <c>false</c>.
        /// </value>
        bool UseMinimumStaffing { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [use maximum staffing].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if use; otherwise, <c>false</c>.
        /// </value>
        bool UseMaximumStaffing { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [use maximum seats].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if use; otherwise, <c>false</c>.
        /// </value>
        bool UseMaximumSeats { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [do not break maximum seats].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if use; otherwise, <c>false</c>.
        /// </value>
        bool DoNotBreakMaximumSeats { get; set; }

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
