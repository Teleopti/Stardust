using System;
using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject
{
    /// <summary>
    /// Class containing details about restrictions for one date, as well as schedule period details.
    /// </summary>
    [DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2010/04/")]
    public class ValidatedSchedulePartDto : IExtensibleDataObject
    {
        /// <summary>
        /// Gets or sets the date.
        /// </summary>
        [DataMember]
        public DateOnlyDto DateOnly { get; set; }
        
        /// <summary>
        /// Gets or sets the legal state.
        /// </summary>
        [DataMember]
        public bool LegalState { get; set; }

        /// <summary>
        /// Gets or sets an indication whether this day is scheduled with day off.
        /// </summary>
        [DataMember]
        public bool HasDayOff { get; set; }

        /// <summary>
        /// Gets or sets an indication whether this day is scheduled with a shift.
        /// </summary>
        [DataMember]
        public bool HasShift { get; set; }

        /// <summary>
        /// Gets or sets an indication whether this day has an absence set.
        /// </summary>
        [DataMember]
        public bool HasAbsence { get; set; }

        /// <summary>
        /// Gets or sets the minimum work time in minutes for this schedule period.
        /// </summary>
        [DataMember]
        public int MinWorkTimeInMinutes { get; set; }

        /// <summary>
        /// Gets or sets the maximum work time in minutes for this schedule period.
        /// </summary>
        [DataMember]
        public int MaxWorkTimeInMinutes { get; set; }

        /// <summary>
        /// Gets or sets the earliest start time in minutes with the given restrictions.
        /// </summary>
        [DataMember]
        public int MinStartTimeMinute { get; set; }

        /// <summary>
        /// Gets or sets the latest start time in minutes with the given restrictions.
        /// </summary>
        [DataMember]
        public int MaxStartTimeMinute { get; set; }
        
        /// <summary>
        /// Gets or sets the earliest end time in minutes with the given restrictions.
        /// </summary>
        [DataMember]
        public int MinEndTimeMinute { get; set; }
        
        /// <summary>
        /// Gets or sets the latest end time in minutes with the given restrictions.
        /// </summary>
        [DataMember]
        public int MaxEndTimeMinute { get; set; }

        /// <summary>
        /// Gets or sets the indication whether this day falls inside the schedule period.
        /// </summary>
        [DataMember]
        public bool IsInsidePeriod { get; set; }

        /// <summary>
        /// Gets or sets the maximal weekly working hours in minutes.
        /// </summary>
        [DataMember]
        public int WeekMaxInMinutes { get; set; }

        /// <summary>
        /// Gets or sets an indication whether it is possible to set preferences for this day.
        /// </summary>
        [DataMember, Obsolete("Use IsPreferenceEditable")]
        public bool IsEditable { get; set; }
       
        /// <summary>
        /// Gets or sets the period target for the schedule period in minutes.
        /// </summary>
        [DataMember]
        public int PeriodTargetInMinutes { get; set; }

        /// <summary>
        /// Gets or sets the target number of days off.
        /// </summary>
        [DataMember]
        public int PeriodDayOffsTarget { get; set; }

        /// <summary>
        /// Gets or sets the current number of days off for the schedule period.
        /// </summary>
        [DataMember]
        public int PeriodDayOffs { get; set; }

        /// <summary>
        /// Gets or sets the availble number of high priority preferences.
        /// </summary>
        [DataMember]
        public int MustHave{get;set;}

        /// <summary>
        /// Gets or sets the preference.
        /// </summary>
        [DataMember]
        public PreferenceRestrictionDto PreferenceRestriction { get; set; }
        
        /// <summary>
        /// Gets or sets the display color for this day.
        /// </summary>
        [DataMember]
        public ColorDto DisplayColor { get; set; } 

        /// <summary>
        /// Gets or sets the name of the scheduled item.
        /// </summary>
        [DataMember]
        public string ScheduledItemName { get; set; }

        /// <summary>
        /// Gets or sets the short name of the scheduled item.
        /// </summary>
        [DataMember]
        public string ScheduledItemShortName { get; set; }

        /// <summary>
        /// Gets or sets the seasonality
        /// </summary>
        [DataMember]
        public double Seasonality { get; set; }

        /// <summary>
        /// Gets or sets the tool tip text for this day.
        /// </summary>
        [DataMember(IsRequired = false, Order = 1)]
        public string TipText { get; set; }

        /// <summary>
        /// Gets or sets an indication if this day only has personal assignments.
        /// </summary>
        [DataMember(IsRequired = false, Order = 1)]
        public bool HasPersonalAssignmentOnly { get; set; }

        /// <summary>
        /// Gets or sets the period target with balance calculated in minutes.
        /// </summary>
        [DataMember(IsRequired = false, Order = 2)]
        public int BalancedPeriodTargetInMinutes { get; set; }

        /// <summary>
        /// Gets or sets the balance in, in minutes.
        /// </summary>
        [DataMember(IsRequired = false, Order = 2)]
        public int BalanceInInMinutes { get; set; }

        /// <summary>
        /// Gets or sets the extra minutes for this schedule period.
        /// </summary>
        [DataMember(IsRequired = false, Order = 2)]
        public int ExtraInInMinutes { get; set; }

        /// <summary>
        /// Gets or sets the balance out, in minutes.
        /// </summary>
        [DataMember(IsRequired = false, Order = 2)]
        public int BalanceOutInMinutes { get; set; }

        /// <summary>
        /// Gets or sets the student availability details for this day.
        /// </summary>
        [DataMember(IsRequired = false, Order = 3)]
        public StudentAvailabilityDayDto StudentAvailabilityDay { get; set; }

        /// <summary>
        /// Gets or sets an indication whether it is possible to set preferences for this day.
        /// </summary>
        [DataMember(IsRequired = false, Order = 3)]
        public bool IsPreferenceEditable { get; set; }

        /// <summary>
        /// Gets or sets an indication whether it is possible to set student availability for this day.
        /// </summary>
        [DataMember(IsRequired = false, Order = 3)]
        public bool IsStudentAvailabilityEditable { get; set; }

        /// <summary>
        /// Gets or sets the negative tolerance for period work time in minutes.
        /// </summary>
        [DataMember(IsRequired = false, Order = 4)]
        public int TargetTimeNegativeToleranceInMinutes { get; set; }

        /// <summary>
        /// Gets or sets the positive tolerance for period work time in minutes.
        /// </summary>
        [DataMember(IsRequired = false, Order = 4)]
        public int TargetTimePositiveToleranceInMinutes { get; set; }

        /// <summary>
        /// Gets or sets an indication whether this day has a contract day off.
        /// </summary>
        [DataMember(IsRequired = false, Order = 4)]
        public bool IsContractDayOff { get; set; }

		/// <summary>
		/// Gets or sets an indication whether this day will break nightly rest rule.
		/// </summary>
		[DataMember(IsRequired = false, Order = 5)]
		public bool ViolatesNightlyRest { get; set; }

        /// <summary>
        /// Gets or sets if this is a contract schedule work day
        /// </summary>
        [DataMember(IsRequired = false, Order = 5)]
        public bool IsWorkday { get; set; }

		/// <summary>
		/// Gets or sets the minimum contract time in minutes for this schedule period.
		/// </summary>
		[DataMember(IsRequired = false, Order = 6)]
		public int MinContractTimeInMinutes { get; set; }

		/// <summary>
		/// /// Gets or sets the maximum contract time in minutes for this schedule period.
		/// </summary>
		[DataMember(IsRequired = false, Order = 6)]
		public int MaxContractTimeInMinutes { get; set; }

		/// <summary>
		/// Internal data for version compatibility.
		/// </summary>
        public ExtensionDataObject ExtensionData
        {
            get;
            set;
        }
    }
}
