using System;
using System.Collections.Generic;
using System.ComponentModel;
using Teleopti.Ccc.Secrets.WorkShiftCalculator;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	/// <summary>
	/// 
	/// </summary>
	public enum ScheduleEmploymentType
    {
        /// <summary>
        /// Schedule only Employed on fixed basis
        /// </summary>
        FixedStaff,
        /// <summary>
        /// Schedule only Employed at hourly basis
        /// </summary>
        HourlyStaff
    }

    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// Created by: peterwe
    /// Created date: 2009-01-21
    /// </remarks>
    public interface ISchedulingOptions : ICloneable
    {
        /// <summary>
        /// Gets or sets a value indicating whether [use minimum persons].
        /// </summary>
        /// <value><c>true</c> if [use minimum persons]; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2009-01-21
        /// </remarks>
        bool UseMinimumPersons { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [use maximum persons].
        /// </summary>
        /// <value><c>true</c> if [use maximum persons]; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2009-01-21
        /// </remarks>
        bool UseMaximumPersons { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [use preferences].
        /// </summary>
        /// <value><c>true</c> if [use preferences]; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2009-01-21
        /// </remarks>
        bool UsePreferences { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to use preferences must have].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [use preferences must have]; otherwise, <c>false</c>.
        /// </value>
        bool UsePreferencesMustHaveOnly { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [preferences days only].
        /// </summary>
        /// <value><c>true</c> if [preferences days only]; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2009-01-21
        /// </remarks>
        bool PreferencesDaysOnly { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [use rotations].
        /// </summary>
        /// <value><c>true</c> if [use rotations]; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2009-01-21
        /// </remarks>
        bool UseRotations { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [rotation days only].
        /// </summary>
        /// <value><c>true</c> if [rotation days only]; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2009-01-21
        /// </remarks>
        bool RotationDaysOnly { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [use availability].
        /// </summary>
        /// <value><c>true</c> if [use availability]; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2009-01-21
        /// </remarks>
        bool UseAvailability { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [availability days only].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [availability days only]; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2009-01-21
        /// </remarks>
        bool AvailabilityDaysOnly { get; set; }

        /// <summary>
        /// Gets or sets the shift category.
        /// </summary>
        /// <value>The shift category.</value>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2009-01-21
        /// </remarks>
        IShiftCategory ShiftCategory { get; set; }

        /// <summary>
        /// Gets or sets the day off template.
        /// </summary>
        /// <value>The day off template.</value>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2009-03-30
        /// </remarks>
        IDayOffTemplate DayOffTemplate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [only shifts when understaffed].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [only shifts when understaffed]; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2009-01-21
        /// </remarks>
        bool OnlyShiftsWhenUnderstaffed { get; set; }
        /// <summary>
        /// Gets or sets the type of the schedule employment.
        /// </summary>
        /// <value>The type of the schedule employment.</value>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2009-01-21
        /// </remarks>
        ScheduleEmploymentType ScheduleEmploymentType { get; set; }
        /// <summary>
        /// Gets or sets the work shift length hint, whether the service should find a longer or a shorter workshift if possible.
        /// </summary>
        /// <value>The work shift length hint option.</value>
        WorkShiftLengthHintOption WorkShiftLengthHintOption { get; set; }

        /// <summary>
        /// Gets or sets the refresh rate.
        /// </summary>
        /// <value>The refresh rate.</value>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2009-03-05
        /// </remarks>
        int RefreshRate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [use student availability].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [use student availability]; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2009-04-06
        /// </remarks>
        bool UseStudentAvailability { get; set; }

        /// <summary>
        /// Gets or sets the not allowed shift categories.
        /// </summary>
        /// <value>The not allowed shift categories.</value>
        IList<IShiftCategory> NotAllowedShiftCategories { get; }

        /// <summary>
        /// Gets or sets a value indicating whether to use shift category limitations.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [use shift category limitations]; otherwise, <c>false</c>.
        /// </value>
        bool UseShiftCategoryLimitations { get; set; }

        ///// <summary>
        ///// Gets or sets the use block scheduling.
        ///// </summary>
        ///// <value>The use block scheduling.</value>
        //BlockFinderType UseBlockScheduling { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether [use group scheduling].
		/// </summary>
		/// <value><c>true</c> if [use group scheduling]; otherwise, <c>false</c>.</value>
		bool UseTeam { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to consider short breaks.
        /// </summary>
        /// <value><c>true</c> if [consider short breaks]; otherwise, <c>false</c>.</value>
        [DefaultValue(true)]
        bool ConsiderShortBreaks { get; set; }

        /// <summary>
        /// Gets or sets a value indicating if same days off should be used when optimizing teams
        /// </summary>
        bool UseSameDayOffs { get; set; }
        IScheduleTag TagToUseOnScheduling { get; set; }

        ///<summary>
        ///</summary>
        bool ShowTroubleshot { get; set; }
        
        /// <summary>
		/// Gets or sets the resource calculate frequency.
		/// </summary>
		/// <value>
		/// The resource calculate frequency.
		/// </value>
		int ResourceCalculateFrequency { get; set; }

		/// <summary>
		/// Gets or sets the use custom target time.
		/// </summary>
		/// <value>
		/// The use custom target time.
		/// </value>
		TimeSpan? UseCustomTargetTime { get; set; }

        /// <summary>
        /// 
        /// </summary>
		ISpecification<IEditableShift> MainShiftOptimizeActivitySpecification
		{
			get;
			set;
		}
		
        /// <summary>
		/// 
		/// </summary>
		bool TeamSameStartTime { get; set; }
		/// <summary>
		/// 
		/// </summary>
		bool TeamSameEndTime { get; set; }
		/// <summary>
		/// 
		/// </summary>
		bool TeamSameShiftCategory { get; set; }

        /// <summary>
        /// Common Activty
        /// </summary>
        bool TeamSameActivity { get; set; }

        /// <summary>
        /// Common Activity
        /// </summary>
        IActivity CommonActivity { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether [use average shift lengths].
		/// </summary>
		/// <value>
		/// 	<c>true</c> if [use average shift lengths]; otherwise, <c>false</c>.
		/// </value>
		bool UseAverageShiftLengths { get; set; }

        BlockFinderType BlockFinderTypeForAdvanceScheduling { get; set; }

        GroupPageLight GroupOnGroupPageForTeamBlockPer { get; set; }

        /// <summary>
        /// Used in TeamBlock per same end time
        /// </summary>
        bool BlockSameEndTime { get; set; }

        /// <summary>
        /// Used in TeamBlock per same shift category
        /// </summary>
        bool BlockSameShiftCategory { get; set; }

        /// <summary>
        /// Used in TeamBlock per same start time
        /// </summary>
        bool BlockSameStartTime { get; set; }

        /// <summary>
        /// Used in TeamBlock per same shift
        /// </summary>
		  bool BlockSameShift { get; set; }

        /// <summary>
        /// Used if the TeamBlock option is checked or not
        /// </summary>
		  bool UseBlock { get; set; }

	    void AddNotAllowedShiftProjectionCache(IShiftProjectionCache shiftProjectionCache);
		IList<IShiftProjectionCache> NotAllowedShiftProjectionCaches { get; }
	    IRuleSetBag FixedShiftBag { get; set; }
	    void ClearNotAllowedShiftProjectionCaches();
		IMultiplicatorDefinitionSet OvertimeType { get; set; }
	    bool ScheduleOnDayOffs { get; set; }
		bool AllowBreakContractTime { get; set; }
	    bool SkipNegativeShiftValues { get; set; }
	    bool ShiftBagBackToLegal { get; set; }
	    IBlockFinder BlockFinder();
	    bool IsDayScheduled(IScheduleDay scheduleDay);
    }
}
